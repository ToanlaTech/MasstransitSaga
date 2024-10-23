import React, { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { v4 as uuidv4 } from 'uuid'; // Để auto-generate orderId

const OrderForm: React.FC = () => {
    const [productId, setProductId] = useState<number>(1);
    const [quantity, setQuantity] = useState<number>(1);
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [orderStatus, setOrderStatus] = useState<string>('');
    const [orderMessage, setOrderMessage] = useState<string>('');

    // Khởi tạo kết nối SignalR
    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl("/hub/orderStatusHub")
            .build();

        newConnection.start()
            .then(() => {
                console.log("SignalR connected.");
                setConnection(newConnection);
            })
            .catch(err => console.error("Error starting SignalR connection:", err));

        return () => {
            if (newConnection) {
                newConnection.stop()
                    .then(() => console.log('SignalR connection stopped'))
                    .catch(err => console.error('Error stopping connection:', err));
            }
        };
    }, []);

    // Lắng nghe các sự kiện từ SignalR
    useEffect(() => {
        if (connection) {
            connection.on("OrderSubmitted", (data: any) => {
                console.log("Order submitted:", data);
                setOrderStatus("OrderSubmitted");
                setOrderMessage(`Order submitted: ${data.message}`);
            });

            connection.on("OrderRejected", (data: any) => {
                console.log("Order rejected:", data);
                setOrderStatus("OrderRejected");
                setOrderMessage(`Order rejected: ${data.message}`);

            });

            connection.on("OrderAccepted", (data: any) => {
                console.log("Order accepted:", data);
                setOrderStatus("OrderAccepted");
                setOrderMessage(`Order accepted: ${data.message}`);
            });

            connection.on("OrderCompleted", (data: any) => {
                console.log("Order completed:", data);
                setOrderStatus("OrderCompleted");
                setOrderMessage(`Order completed: ${data.message}`);
            });

            return () => {
                connection.off("OrderSubmitted");
                connection.off("OrderRejected");
                connection.off("OrderAccepted");
                connection.off("OrderCompleted");
            };
        }
    }, [connection]);

    // Xử lý submit form và gửi POST request
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        // Generate orderId tự động
        const generatedOrderId = uuidv4();

        const requestBody = {
            orderId: generatedOrderId,
            productId: productId,
            quantity: quantity
        };

        try {
            const response = await fetch('/api/orders/submit', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestBody)
            });

            if (response.ok) {
                console.log("Order submitted successfully!");
                setOrderStatus("OrderSubmitted");
                setOrderMessage(`Order submitted.`);
                // Nếu submit thành công, sử dụng orderId để join vào SignalR group
                if (connection) {
                    connection.invoke("JoinOrderGroup", generatedOrderId)
                        .then(() => console.log(`Joined group with Order ID: ${generatedOrderId}`))
                        .catch(err => console.error('Error joining group:', err));
                }
            } else {
                console.error("Failed to submit order");
            }
        } catch (error) {
            console.error("Error during submission:", error);
        }
    };

    return (
        <div className="container mt-5">
            <h2 className="text-center mb-4">Order Form</h2>
            <form onSubmit={handleSubmit}>
                <div className="form-group mb-3">
                    <label htmlFor="productId">Product ID</label>
                    <input
                        type="number"
                        className="form-control"
                        id="productId"
                        value={productId}
                        onChange={(e) => setProductId(Number(e.target.value))}
                    />
                </div>
                <div className="form-group mb-3">
                    <label htmlFor="quantity">Quantity</label>
                    <input
                        type="number"
                        className="form-control"
                        id="quantity"
                        value={quantity}
                        onChange={(e) => setQuantity(Number(e.target.value))}
                    />
                </div>
                <button type="submit" className="btn btn-primary w-100">Submit Order</button>
            </form>


            {orderStatus && (
                <div
                    className={`alert ${orderStatus === 'OrderCompleted'
                        ? 'alert-success'
                        : orderStatus === 'OrderSubmitted'
                            ? 'alert-info'
                            : orderStatus.includes('OrderRejected')
                                ? 'alert-danger'
                                : 'alert-warning'
                        } mt-4`}
                    role="alert"
                >
                    {orderMessage}
                </div>
            )}

        </div>
    );
};

export default OrderForm;
