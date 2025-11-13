import React, { useState, useRef } from 'react';
import {
  Form,
  Button,
  Card,
  Row,
  Col,
  Alert,
  ButtonGroup
} from 'react-bootstrap';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import Captcha from '../Captcha/Captcha';
import FileUpload from '../FileUpload/FileUpload';
import { commentService } from '../../services/api';
import './CommentForm.css';

const CommentForm = ({ onCommentAdded, parentId = null, compact = false }) => {
  const [captcha, setCaptcha] = useState({ id: '', image: '' });
  const [preview, setPreview] = useState(false);
  const [submitLoading, setSubmitLoading] = useState(false);
  const [submitError, setSubmitError] = useState('');
  const [showForm, setShowForm] = useState(true); // Новое состояние для показа формы
  const [successMessage, setSuccessMessage] = useState(false); // Для сообщения об успехе
  const fileInputRef = useRef(null);

  const validationSchema = Yup.object({
    userName: Yup.string()
        .required('Username is required')
        .min(3, 'Username must be at least 3 characters')
        .max(50, 'Username must be at most 50 characters')
        .matches(/^[a-zA-Z0-9]+$/, 'Only latin letters and numbers'),
    email: Yup.string()
        .email('Invalid email format')
        .required('Email is required')
        .max(100, 'Email must be at most 100 characters'),
    homePage: Yup.string()
        .url('Invalid URL format')
        .max(500, 'URL must be at most 500 characters'),
    text: Yup.string()
        .required('Comment text is required')
        .min(1, 'Comment must have at least 1 character')
        .max(5000, 'Comment must be at most 5000 characters'),
    captchaCode: Yup.string()
        .required('CAPTCHA is required')
        .min(4, 'CAPTCHA must have at least 4 characters')
        .max(10, 'CAPTCHA must have at most 10 characters'),
    file: Yup.mixed().nullable()
        .test('fileSize', 'File is too large', (value) => {
          if (!value) return true;
          return value.size <= 5 * 1024 * 1024;
        })
  });

  const formik = useFormik({
    initialValues: {
      userName: '',
      email: '',
      homePage: '',
      text: '',
      captchaCode: '',
      file: null,
      parentId: parentId
    },
    validationSchema,
    onSubmit: async (values, { resetForm, setErrors }) => {
      try {
        setSubmitLoading(true);
        setSubmitError('');

        const formData = new FormData();
        Object.entries(values).forEach(([key, value]) => {
          if (value !== null && value !== '' && value !== undefined) {
            const capitalKey = key.charAt(0).toUpperCase() + key.slice(1);
            formData.append(capitalKey, value);
          }
        });

        const response = await commentService.createComment(formData);

        if (response.success) {
          resetForm();
          if (fileInputRef.current) fileInputRef.current.value = '';
          setCaptcha({ id: '', image: '' });
          setPreview(false);
          onCommentAdded(response.data);
          setShowForm(false); // Скрываем форму после успеха
          setSuccessMessage(true); // Показываем сообщение об успехе
        } else {
          setSubmitError(response.message || 'Failed to submit comment');
        }
      } catch (err) {
        setSubmitError(err.message || 'Error submitting comment');
      } finally {
        setSubmitLoading(false);
      }
    }
  });

  const handleFileSelect = (file) => {
    formik.setFieldValue('file', file);
  };

  if (!showForm) {
    return (
        <Alert variant="success" className="mt-3">
          Comment added successfully!
          <Button
              variant="link"
              onClick={() => {
                setShowForm(true);
                setSuccessMessage(false);
              }}
              className="ms-2 p-0"
          >
            Add another
          </Button>
        </Alert>
    );
  }

  return (
      <Card className={`comment-form ${compact ? 'compact' : ''} ${parentId ? 'reply-form' : ''}`}>
        <Card.Body>
          <Form onSubmit={formik.handleSubmit}>
            {!compact && <Card.Title>Add Comment</Card.Title>}

            <Row>
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>Username *</Form.Label>
                  <Form.Control
                      type="text"
                      name="userName"
                      value={formik.values.userName}
                      onChange={formik.handleChange}
                      onBlur={formik.handleBlur}
                      isInvalid={formik.touched.userName && formik.errors.userName}
                      disabled={submitLoading}
                      placeholder="Enter username"
                  />
                  <Form.Control.Feedback type="invalid">
                    {formik.errors.userName}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>

              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>Email *</Form.Label>
                  <Form.Control
                      type="email"
                      name="email"
                      value={formik.values.email}
                      onChange={formik.handleChange}
                      onBlur={formik.handleBlur}
                      isInvalid={formik.touched.email && formik.errors.email}
                      disabled={submitLoading}
                      placeholder="Enter email"
                  />
                  <Form.Control.Feedback type="invalid">
                    {formik.errors.email}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>

              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>Home Page</Form.Label>
                  <Form.Control
                      type="url"
                      name="homePage"
                      value={formik.values.homePage}
                      onChange={formik.handleChange}
                      onBlur={formik.handleBlur}
                      isInvalid={formik.touched.homePage && formik.errors.homePage}
                      disabled={submitLoading}
                      placeholder="https://example.com"
                  />
                  <Form.Control.Feedback type="invalid">
                    {formik.errors.homePage}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
            </Row>

            <Form.Group className="mb-3">
              <Form.Label>Comment Text *</Form.Label>
              <Form.Control
                  as="textarea"
                  rows={compact ? 2 : 4}
                  name="text"
                  value={formik.values.text}
                  onChange={formik.handleChange}
                  onBlur={formik.handleBlur}
                  isInvalid={formik.touched.text && formik.errors.text}
                  disabled={submitLoading}
                  placeholder="Enter your comment here. Supports basic HTML: <a>, <code>, <i>, <strong>."
              />
              <Form.Control.Feedback type="invalid">
                {formik.errors.text}
              </Form.Control.Feedback>
            </Form.Group>

            <FileUpload
                onFileSelect={handleFileSelect}
                disabled={submitLoading}
                ref={fileInputRef}
            />

            <Captcha
                captcha={captcha}
                setCaptcha={setCaptcha}
                value={formik.values.captchaCode}
                onChange={(value) => formik.setFieldValue('captchaCode', value)}
                error={formik.errors.captchaCode}
                touched={formik.touched.captchaCode}
                disabled={submitLoading}
            />

            {submitError && (
                <Alert variant="danger" className="mt-3">
                  {submitError}
                </Alert>
            )}

            <div className="d-flex gap-2 mt-3 flex-wrap">
              <Button
                  variant="primary"
                  type="submit"
                  disabled={submitLoading || !captcha.id}
                  className="flex-fill"
              >
                {submitLoading ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2" />
                      Submitting...
                    </>
                ) : (
                    'Submit Comment'
                )}
              </Button>

              {!compact && (
                  <Button
                      variant="outline-secondary"
                      type="button"
                      onClick={() => setPreview(!preview)}
                      disabled={submitLoading}
                  >
                    {preview ? 'Hide Preview' : 'Show Preview'}
                  </Button>
              )}
            </div>
          </Form>

          {preview && (
              <div className="mt-3 p-3 border rounded preview-section">
                <h6>Comment Preview:</h6>
                <div className="preview-content">
                  <div className="d-flex justify-content-between align-items-start mb-2">
                    <strong>{formik.values.userName || 'Username'}</strong>
                    <small className="text-muted">Now</small>
                  </div>
                  <div
                      className="comment-text"
                      dangerouslySetInnerHTML={{
                        __html: formik.values.text || '<em>Comment text</em>'
                      }}
                  />
                </div>
              </div>
          )}
        </Card.Body>
      </Card>
  );
};

export default CommentForm;