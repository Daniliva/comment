// File: src/components/CommentForm/CommentForm.jsx
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
        formData.append('CaptchaId', captcha.id);

        const response = await commentService.createComment(formData);

        if (response.success) {
          onCommentAdded(response.data);
          resetForm();
          setCaptcha({ id: '', image: '' });
          setPreview(false);
          if (fileInputRef.current) {
            fileInputRef.current.value = '';
          }
        } else {
          setSubmitError(response.message || 'Error submitting comment');
        }
      } catch (error) {
        console.error('Submit error:', error);
        setSubmitError(error.response?.data?.message || error.message || 'Error submitting comment');

        if (error.response?.data?.errors) {
          const serverErrors = {};
          Object.entries(error.response.data.errors).forEach(([key, msgs]) => {
            serverErrors[key.toLowerCase()] = msgs.join(', ');
          });
          setErrors(serverErrors);
        }
      } finally {
        setSubmitLoading(false);
      }
    }
  });

  const handleFileSelect = (file) => {
    formik.setFieldValue('file', file);
  };

  const handleTextFormat = (tag) => {
    const textarea = document.querySelector('textarea[name="text"]');
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selectedText = formik.values.text.substring(start, end);

    let newText = formik.values.text;
    if (tag === 'a') {
      const url = prompt('Enter link URL:');
      if (url) newText = newText.slice(0, start) + `<a href="${url}">${selectedText || 'link'}</a>` + newText.slice(end);
    } else {
      newText = newText.slice(0, start) + `<${tag}>${selectedText}</${tag}>` + newText.slice(end);
    }
    formik.setFieldValue('text', newText);
  };

  return (
      <Card className={`comment-form ${compact ? 'compact' : ''}`}>
        <Card.Header>
          <h5 className="mb-0">{compact ? 'Reply to Comment' : 'Add New Comment'}</h5>
        </Card.Header>
        <Card.Body>
          <Form onSubmit={formik.handleSubmit}>
            <Row>
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>Username *</Form.Label>
                  <Form.Control
                      name="userName"
                      placeholder="Enter your username"
                      value={formik.values.userName}
                      onChange={formik.handleChange}
                      onBlur={formik.handleBlur}
                      isInvalid={formik.touched.userName && formik.errors.userName}
                      disabled={submitLoading}
                      maxLength={50}
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
                      placeholder="Enter your email"
                      value={formik.values.email}
                      onChange={formik.handleChange}
                      onBlur={formik.handleBlur}
                      isInvalid={formik.touched.email && formik.errors.email}
                      disabled={submitLoading}
                      maxLength={100}
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
                      name="homePage"
                      placeholder="Enter your website URL"
                      value={formik.values.homePage}
                      onChange={formik.handleChange}
                      onBlur={formik.handleBlur}
                      isInvalid={formik.touched.homePage && formik.errors.homePage}
                      disabled={submitLoading}
                      maxLength={500}
                  />
                  <Form.Control.Feedback type="invalid">
                    {formik.errors.homePage}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
            </Row>

            <Form.Group className="mb-3">
              <Form.Label>Comment Text *</Form.Label>
              <ButtonGroup className="mb-2">
                <Button
                    variant="outline-secondary"
                    type="button"
                    onClick={() => handleTextFormat('a')}
                    title="Link"
                >
                  🔗
                </Button>
                <Button
                    variant="outline-secondary"
                    type="button"
                    onClick={() => handleTextFormat('code')}
                    title="Code"
                >
                  &lt;/&gt;
                </Button>
                <Button
                    variant="outline-secondary"
                    type="button"
                    onClick={() => handleTextFormat('strong')}
                    title="Bold"
                >
                  <strong>B</strong>
                </Button>
                <Button
                    variant="outline-secondary"
                    type="button"
                    onClick={() => handleTextFormat('i')}
                    title="Italic"
                >
                  <i>I</i>
                </Button>
              </ButtonGroup>

              <Form.Control
                  as="textarea"
                  rows={compact ? 3 : 4}
                  name="text"
                  placeholder="Enter your comment..."
                  value={formik.values.text}
                  onChange={formik.handleChange}
                  onBlur={formik.handleBlur}
                  isInvalid={formik.touched.text && formik.errors.text}
                  disabled={submitLoading}
              />
              <Form.Text className="text-muted">
                Supported HTML tags: &lt;a href=""&gt;, &lt;code&gt;, &lt;i&gt;, &lt;strong&gt;
              </Form.Text>
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