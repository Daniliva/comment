// File: src/components/FileUpload/FileUpload.jsx
import React, { forwardRef, useState } from 'react';
import { Form, Alert, Badge } from 'react-bootstrap';
import './FileUpload.css';

const FileUpload = forwardRef(({ onFileSelect, disabled }, ref) => {
    const [selectedFile, setSelectedFile] = useState(null);
    const [error, setError] = useState('');

    const handleFileChange = (event) => {
        const file = event.target.files[0];
        setError('');

        if (!file) {
            setSelectedFile(null);
            onFileSelect(null);
            return;
        }

        const allowedImageTypes = ['image/jpeg', 'image/png', 'image/gif'];
        const allowedTextTypes = ['text/plain'];

        if (file.type.startsWith('image/')) {
            if (!allowedImageTypes.includes(file.type)) {
                setError('Allowed image formats: JPG, PNG, GIF');
                return;
            }
            if (file.size > 5 * 1024 * 1024) {
                setError('Image size must not exceed 5MB');
                return;
            }
        } else if (file.type === 'text/plain') {
            if (file.size > 100 * 1024) {
                setError('Text file size must not exceed 100KB');
                return;
            }
        } else {
            setError('Allowed file formats: JPG, PNG, GIF, TXT');
            return;
        }

        setSelectedFile(file);
        onFileSelect(file);
    };

    const formatFileSize = (bytes) => {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    };

    const getFileTypeBadge = (file) => {
        if (file.type.startsWith('image/')) {
            return <Badge bg="success">Image</Badge>;
        } else if (file.type === 'text/plain') {
            return <Badge bg="info">Text file</Badge>;
        }
        return <Badge bg="secondary">File</Badge>;
    };

    return (
        <Form.Group className="mb-3 file-upload">
            <Form.Label>
                Attach file <small className="text-muted">(optional)</small>
            </Form.Label>

            <Form.Control
                ref={ref}
                type="file"
                onChange={handleFileChange}
                disabled={disabled}
                accept=".jpg,.jpeg,.png,.gif,.txt,image/jpeg,image/png,image/gif,text/plain"
                className="file-input"
            />

            {selectedFile && (
                <div className="file-info mt-2 p-2 border rounded">
                    <div className="d-flex justify-content-between align-items-center">
                        <div>
                            <strong>{selectedFile.name}</strong>
                            <div className="small text-muted">
                                {getFileTypeBadge(selectedFile)} • {formatFileSize(selectedFile.size)}
                            </div>
                        </div>
                        <button
                            type="button"
                            className="btn-close btn-close-sm"
                            onClick={() => {
                                setSelectedFile(null);
                                onFileSelect(null);
                                if (ref.current) ref.current.value = '';
                            }}
                            disabled={disabled}
                        />
                    </div>
                </div>
            )}

            {error && (
                <Alert variant="danger" className="mt-2 small">
                    {error}
                </Alert>
            )}

            <Form.Text className="text-muted">
                Images: JPG, PNG, GIF (max. 320×240px, 5MB). Text files: TXT (max. 100KB).
            </Form.Text>
        </Form.Group>
    );
});

FileUpload.displayName = 'FileUpload';

export default FileUpload;