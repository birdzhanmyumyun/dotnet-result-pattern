// JavaScript source code
import { check } from 'k6';
import http from 'k6/http';

export const options = {
    stages: [
        { duration: '10s', target: 20 },
        { duration: '50s', target: 20 },
    ],
};

export default function () {
    const apiUrl = 'http://localhost:5161/exceptions/todos/f9b4d19c-5d54-44e5-a499-e2e245bf412d';

    const response = http.get(apiUrl);

    check(response, {
        'is status 500': (r) => r.status === 500,
    });
};