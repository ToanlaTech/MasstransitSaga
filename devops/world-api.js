import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
    scenarios: {
        ramping_requests: {
            executor: 'ramping-vus',
            stages: [
                { duration: '5s', target: 20 },  // 5s, 20 VUs
                { duration: '10s', target: 100 }, // 10s, 100 VUs
                { duration: '20s', target: 80 },  // 20s, 80 VUs
                { duration: '7s', target: 200 },  // 7s, 200 VUs
            ],
        },
    },
};

export default function () {
    http.get('http://localhost:8083/api/todos?limit=10&skip=0'); // Thay URL API của bạn
    sleep(1);
}
