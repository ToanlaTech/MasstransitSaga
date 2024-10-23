import React, { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import axios from 'axios';

// Thành phần OrderForm xử lý logic của từng đơn hàng
const OrderForm: React.FC<{ formId: number }> = ({ formId }) => {
    const [productId, setProductId] = useState<number>(1);
    const [quantity, setQuantity] = useState<number>(1);
    const [orderStatus, setOrderStatus] = useState<string>('Pending');
    const [orderMessage, setOrderMessage] = useState<string>('Submitting order...');
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [orderId, setOrderId] = useState<string>('');

    useEffect(() => {
        const newOrderId = `order-${formId}-${new Date().getTime()}`;
        setOrderId(newOrderId);

        // Tạo kết nối SignalR
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl("/hub/orderStatusHub")
            .build();

        newConnection.start()
            .then(() => {
                console.log(`SignalR connected for form ${formId}.`);
                setConnection(newConnection);
            })
            .catch(err => console.error("Error starting SignalR connection:", err));

        return () => {
            if (connection) {
                connection.stop().then(() => console.log(`SignalR connection stopped for form ${formId}.`));
            }
        };
    }, [formId]);

    useEffect(() => {
        if (connection) {
            connection.on("OrderSubmitted", (data: any) => {
                console.log(`OrderSubmitted for form ${formId}: `, data);
                setOrderStatus('OrderSubmitted');
                setOrderMessage(data.message);
            });

            connection.on("OrderRejected", (data: any) => {
                console.log(`OrderRejected for form ${formId}: `, data);
                setOrderStatus('OrderRejected');
                setOrderMessage(data.message);
            });

            connection.on("OrderAccepted", (data: any) => {
                console.log(`OrderAccepted for form ${formId}: `, data);
                setOrderStatus('OrderAccepted');
                setOrderMessage(data.message);
            });
        }
    }, [connection, formId]);

    const submitOrder = async () => {
        try {
            await axios.post('/api/orders/submit', {
                orderId,
                productId,
                quantity,
            });
        } catch (error) {
            console.error("Error during order submission:", error);
        }
    };

    return (
        <div className="order-form">
            <h4>Order Form {formId}</h4>
            <div className="form-group mb-3">
                <label htmlFor={`productId-${formId}`}>Product ID</label>
                <input
                    type="number"
                    className="form-control"
                    id={`productId-${formId}`}
                    value={productId}
                    onChange={(e) => setProductId(Number(e.target.value))}
                />
            </div>
            <div className="form-group mb-3">
                <label htmlFor={`quantity-${formId}`}>Quantity</label>
                <input
                    type="number"
                    className="form-control"
                    id={`quantity-${formId}`}
                    value={quantity}
                    onChange={(e) => setQuantity(Number(e.target.value))}
                />
            </div>

            <div className={`alert ${orderStatus === 'OrderRejected' ? 'alert-danger' : 'alert-info'} mt-4`}>
                {`Order ID: ${orderId}, Status: ${orderStatus}, Message: ${orderMessage}`}
            </div>

            <button onClick={submitOrder} className="btn btn-primary">
                Submit Order {formId}
            </button>
        </div>
    );

};

// Thành phần chính chứa 2 form
const MultiOrderForm: React.FC = () => {
    const formRefs = [1, 2]; // Quản lý hai form với id khác nhau

    const handleSubmitBothOrders = () => {
        // Sử dụng ref hoặc trực tiếp gọi các hàm submit từ từng form
        document.getElementById('submitForm1')?.click();
        document.getElementById('submitForm2')?.click();
    };

    return (
        <div className="container mt-5">
            <h2 className="text-center mb-4">Multi Order Form</h2>

            {formRefs.map((formId) => (
                <div key={formId}>
                    <OrderForm formId={formId} />
                    <button id={`submitForm${formId}`} onClick={() => { document.getElementById(`submitForm${formId}`)?.click(); }}>
                        Submit Form {formId}
                    </button>
                </div>
            ))}

            <button onClick={handleSubmitBothOrders} className="btn btn-primary w-100 mt-5">
                Submit Both Orders
            </button>
        </div>
    );
};

export default MultiOrderForm;
