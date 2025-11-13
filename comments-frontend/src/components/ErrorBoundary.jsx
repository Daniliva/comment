// File: src/components/ErrorBoundary.jsx
import React, { Component } from 'react';
import { Alert } from 'react-bootstrap';

class ErrorBoundary extends Component {
    state = { hasError: false, error: null };

    static getDerivedStateFromError(error) {
        return { hasError: true, error };
    }

    componentDidCatch(error, info) {
        console.error('Component error:', error, info);
    }

    render() {
        if (this.state.hasError) {
            return <Alert variant="danger">An error occurred. Please refresh the page or try again.</Alert>;
        }
        return this.props.children;
    }
}

export default ErrorBoundary;