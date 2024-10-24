import React, { useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { v4 as uuidv4 } from 'uuid'; // Để auto-generate orderId
import axios from 'axios';

const OrderForm: React.FC = () => {
    const [productId, setProductId] = useState<number>(1);
    const [quantity, setQuantity] = useState<number>(1);
    const [orders, setOrders] = useState<{ orderId: string, status: string, message: string }[]>([]);

    // Khởi tạo kết nối SignalR
    const createConnections = (orderIds: string[]) => {
        // Đoạn code sửa trong createConnections
        orderIds.forEach(orderId => {
            const newConnection = new signalR.HubConnectionBuilder()
                .withUrl("/hub/orderStatusHub")
                .build();

            newConnection.start()
                .then(() => {
                    console.log(`SignalR connected for order ${orderId}.`);
                })
                .catch(err => console.error("Error starting SignalR connection:", err));

            // Lắng nghe sự kiện từ SignalR cho từng orderId
            newConnection.on("OrderSubmitted", (data: any) => {
                console.log(`OrderSubmitted for ${orderId}: `, data);
                setOrders(prevOrders => {
                    const updatedOrders = prevOrders.map(order =>
                        order.orderId === orderId
                            ? { ...order, status: 'OrderSubmitted', message: data.message }
                            : order
                    );
                    console.log("Updated orders after OrderSubmitted: ", updatedOrders);
                    return updatedOrders;
                });
            });

            newConnection.on("OrderRejected", (data: any) => {
                console.log(`OrderRejected for ${orderId}: `, data);
                setOrders(prevOrders => {
                    const updatedOrders = prevOrders.map(order =>
                        order.orderId === orderId
                            ? { ...order, status: 'OrderRejected', message: data.message }
                            : order
                    );
                    console.log("Updated orders after OrderRejected: ", updatedOrders);
                    return updatedOrders;
                });
            });

            newConnection.on("OrderAccepted", (data: any) => {
                console.log(`OrderAccepted for ${orderId}: `, data);
                setOrders(prevOrders => {
                    const updatedOrders = prevOrders.map(order =>
                        order.orderId === orderId
                            ? { ...order, status: 'OrderAccepted', message: data.message }
                            : order
                    );
                    console.log("Updated orders after OrderAccepted: ", updatedOrders);
                    return updatedOrders;
                });
            });
        });

    };

    // Hàm gửi đồng thời 5 đơn hàng
    const submitOrders = async () => {
        const orderIds = Array.from({ length: 5 }, () => uuidv4());

        // Khởi tạo các trạng thái ban đầu cho mỗi đơn hàng
        setOrders(orderIds.map(orderId => ({
            orderId,
            status: 'Pending',
            message: 'Submitting order...'
        })));

        try {
            // Gửi 5 request đồng thời
            const submitPromises = orderIds.map(orderId => {
                return axios.post('/api/orders/submit', {
                    orderId,
                    productId,
                    quantity,
                });
            });

            await Promise.all(submitPromises);

            // Mở 5 kết nối SignalR với các orderId
            createConnections(orderIds);

        } catch (error) {
            console.error("Error during submission:", error);
        }
    };

    return (
        <div className="container mt-5">
            <h2 className="text-center mb-4">Order Form</h2>
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
            <button onClick={submitOrders} className="btn btn-primary w-100">Submit 5 Orders</button>

            {orders.length > 0 && orders.map((order, index) => (
                <div
                    key={index}
                    className={`alert ${order.status.includes('Rejected') ? 'alert-danger' : 'alert-info'} mt-4`}
                    role="alert"
                >
                    {`Order ID: ${order.orderId}, Status: ${order.status}, Message: ${order.message}`}
                </div>
            ))}
        </div>
    );
};

export default OrderForm;
