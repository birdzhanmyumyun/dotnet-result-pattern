import { check } from 'k6';
import http from 'k6/http';

export const options = {
    stages: [
        { duration: '10s', target: 20 },
        { duration: '50s', target: 20 },
    ],
};

export default function () {
    const apiUrl = 'http://localhost:5161/exceptions/todos';

    const requestBody = JSON.stringify({
        title: 'A todo with a title longer than 50 characters should not be created.'
    });

    const headers = { 'Content-Type': 'application/json' };

    const response = http.post(apiUrl, requestBody, { headers: headers });

    check(response, {
        'is status 500': (r) => r.status === 500,
    });
};