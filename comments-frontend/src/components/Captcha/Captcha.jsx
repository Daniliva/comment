// File: src/components/Captcha/Captcha.jsx
import React, { useEffect, useState } from 'react';
import { Form, Row, Col, Button, Spinner, Alert } from 'react-bootstrap';
import { captchaService } from '../../services/api';
import './Captcha.css';

const Captcha = ({ captcha, setCaptcha, value, onChange, error, touched, disabled }) => {
    const [loading, setLoading] = useState(false);
    const [captchaError, setCaptchaError] = useState('');

    const loadCaptcha = async () => {
        try {
            setLoading(true);
            setCaptchaError('');
            const response = await captchaService.getCaptcha();

            if (response.success) {
                setCaptcha({
                    id: response.data.captchaId,
                    image: response.data.imageData
                });
            } else {
                setCaptchaError('Failed to load CAPTCHA');
            }
        } catch (error) {
            console.error('CAPTCHA loading error:', error);
            setCaptchaError('Error loading CAPTCHA. Please refresh the page.');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadCaptcha();
    }, []);

    return (
        <Form.Group className="mb-3 captcha-container">
            <Form.Label>CAPTCHA *</Form.Label>

            {captchaError && (
                <Alert variant="warning" className="mb-2">
                    {captchaError}
                </Alert>
            )}

            <Row className="align-items-center">
                <Col md={4}>
                    <div className="captcha-image-container">
                        {loading ? (
                            <div className="text-center py-3">
                                <Spinner animation="border" size="sm" />
                                <div className="mt-1 small">Loading CAPTCHA...</div>
                            </div>
                        ) : (
                            captcha.image && (
                                <img
                                    src={`data:image/png;base64,${captcha.image}`}
                                    alt="CAPTCHA"
                                    className="captcha-image"
                                    onClick={loadCaptcha}
                                    style={{ cursor: 'pointer' }}
                                    title="Click to refresh CAPTCHA"
                                />
                            )
                        )}
                    </div>
                </Col>

                <Col md={5}>
                    <Form.Control
                        type="text"
                        value={value}
                        onChange={(e) => onChange(e.target.value)}
                        onBlur={() => {}}
                        isInvalid={touched && error}
                        placeholder="Enter code from image"
                        disabled={disabled || loading || !captcha.id}
                        maxLength={10}
                    />
                    <Form.Control.Feedback type="invalid">
                        {error}
                    </Form.Control.Feedback>
                    <Form.Text className="text-muted">
                        Enter characters from the image
                    </Form.Text>
                </Col>

                <Col md={3}>
                    <Button
                        variant="outline-secondary"
                        onClick={loadCaptcha}
                        disabled={loading || disabled}
                        className="w-100"
                    >
                        {loading ? (
                            <Spinner animation="border" size="sm" />
                        ) : (
                            'Refresh'
                        )}
                    </Button>
                </Col>
            </Row>
        </Form.Group>
    );
};

export default Captcha;