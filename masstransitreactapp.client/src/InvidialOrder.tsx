import React, { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { v4 as uuidv4 } from 'uuid'; // Để auto-generate orderId

const OrderForm: React.FC<{ formId: number, submit: boolean }> = ({ formId, submit }) => {
    const [productId, setProductId] = useState<number>(1);
    const [quantity, setQuantity] = useState<number>(1);
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [orderStatus, setOrderStatus] = useState<string>('');
    const [orderMessage, setOrderMessage] = useState<string>('');

    // Khởi tạo kết nối SignalR
    useEffect(() => {
        const randomProductId = Math.floor(Math.random() * 10) + 1;
        const randomQuantity = Math.floor(Math.random() * 100) + 1;
        setProductId(randomProductId);
        setQuantity(randomQuantity);
        // const newConnection = new signalR.HubConnectionBuilder()
        //     .withUrl("/hub/orderStatusHub")
        //     .build();

        // newConnection.start()
        //     .then(() => {
        //         console.log("SignalR connected.");
        //         console.log(`Form ${formId} connected.`);
        //         setConnection(newConnection);
        //     })
        //     .catch(err => console.error("Error starting SignalR connection:", err));
    }, []);

    // Lắng nghe các sự kiện từ SignalR
    useEffect(() => {
        if (connection) {
            connection.on("OrderSubmitted", (data: any) => {
                console.log("Order submitted:", data);
                setOrderStatus("OrderSubmitted");
                setOrderMessage(`OrderId: ${data.orderid} - Order submitted: ${data.message}`);
            });

            connection.on("OrderRejected", (data: any) => {
                console.log("Order rejected:", data);
                setOrderStatus("OrderRejected");
                setOrderMessage(`OrderId: ${data.orderid} - Order rejected: ${data.message}`);

            });

            connection.on("OrderAccepted", (data: any) => {
                console.log("Order accepted:", data);
                setOrderStatus("OrderAccepted");
                setOrderMessage(`OrderId: ${data.orderid} - Order accepted: ${data.message}`);
            });

            connection.on("OrderCompleted", (data: any) => {
                console.log("Order completed:", data);
                setOrderStatus("OrderCompleted");
                setOrderMessage(`OrderId: ${data.orderid} - Order completed: ${data.message}`);
            });

            return () => {
                connection.off("OrderSubmitted");
                connection.off("OrderRejected");
                connection.off("OrderAccepted");
                connection.off("OrderCompleted");
            };
        }
    }, [connection]);

    useEffect(() => {
        if (submit) {
            handleSubmit({ preventDefault: () => { } } as React.FormEvent);
        }
    }, [submit]);

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
        <div className="mt-5">
            <h2 className="text-center mb-4">Order Form {formId}</h2>
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
                {/* <button type="submit" className="btn btn-primary w-100">Submit Order</button> */}
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

const OrderFormsContainer: React.FC = () => {
    const [submitAll, setSubmitAll] = useState(false); // State để quản lý khi nào submit tất cả các form

    // Gửi yêu cầu submit cho tất cả các form
    const handleSubmitAll = () => {
        setSubmitAll(true);

        // Sau khi submit xong, đặt lại trạng thái submitAll
        setTimeout(() => {
            setSubmitAll(false);
        }, 1000); // Đặt độ trễ nhỏ để đảm bảo tất cả các form đều được submit
    };

    return (
        <div className="container mt-5">
            <h2 className="text-center mb-4">Order Forms</h2>
            <div className="row">
                {
                    Array.from({ length: 10 }, (_, i) => (
                        <div key={i} className="col-md-4">
                            <OrderForm formId={i + 1} submit={submitAll} />
                        </div>
                    ))
                }
            </div>
            <div className="row mt-4">
                <div className="col text-center">
                    <button onClick={handleSubmitAll} className="btn btn-primary w-50">
                        Submit All Orders
                    </button>
                </div>
            </div>
        </div>
    );
};

export default OrderFormsContainer;
