// File: src/App.js
import React, { useState, useEffect } from 'react';
import { Container, Alert, Spinner } from 'react-bootstrap';
import CommentForm from './components/CommentForm/CommentForm';
import CommentList from './components/CommentList/CommentList';
import ErrorBoundary from './components/ErrorBoundary';
import { ThemeProvider } from './components/ThemeContext';
import ThemeToggle from './components/ThemeToggle';
import { signalRService } from './services/signalr';
import { commentService } from './services/api';
import './styles/App.css';

function App() {
    const [comments, setComments] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [pagination, setPagination] = useState({
        page: 1,
        pageSize: 25,
        totalCount: 0,
        totalPages: 1
    });
    const [sortConfig, setSortConfig] = useState({
        sortBy: 'CreatedAt',
        sortDescending: true
    });

    const fetchComments = async () => {
        try {
            setLoading(true);
            setError(null);
            const response = await commentService.getComments(
                pagination.page,
                sortConfig.sortBy,
                sortConfig.sortDescending
            );

            if (response.data?.success) {
                setComments(response.data.data.items);
                setPagination({
                    ...pagination,
                    totalCount: response.data.data.totalCount,
                    totalPages: response.data.data.totalPages
                });
            } else {
                setError('Failed to load comments');
            }
        } catch (err) {
            setError(err.message || 'Error loading comments');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchComments();
        signalRService.start();

        const handleNewComment = (newComment) => {
            setComments(prevComments => {
                if (newComment.parentId) {
                    // Добавляем reply к parent комментарию
                    return prevComments.map(c => {
                        if (c.id === newComment.parentId) {
                            return {
                                ...c,
                                replies: [...(c.replies || []), newComment]
                            };
                        }
                        return c;
                    });
                } else {
                    // Новый корневой комментарий - добавляем в начало
                    return [newComment, ...prevComments];
                }
            });
            // Обновляем общее количество
            setPagination(prev => ({ ...prev, totalCount: prev.totalCount + 1 }));
        };

        const handleDeletedComment = (deletedId) => {
            setComments(prevComments => {
                // Рекурсивно удаляем комментарий или reply
                const removeRecursive = (comments) => {
                    return comments.filter(c => {
                        if (c.id === deletedId) return false;
                        if (c.replies) {
                            c.replies = removeRecursive(c.replies);
                        }
                        return true;
                    });
                };
                return removeRecursive(prevComments);
            });
            // Обновляем общее количество
            setPagination(prev => ({ ...prev, totalCount: Math.max(0, prev.totalCount - 1) }));
        };

        signalRService.on('NewComment', handleNewComment);
        signalRService.on('DeletedComment', handleDeletedComment);

        return () => {
            signalRService.off('NewComment', handleNewComment);
            signalRService.off('DeletedComment', handleDeletedComment);
        };
    }, [pagination.page, sortConfig.sortBy, sortConfig.sortDescending]);

    const handleCommentAdded = (newComment) => {
        // Не нужно ничего, SignalR обработает
    };

    const handleCommentDeleted = () => {
        // Не нужно ничего, SignalR обработает
    };

    const handlePageChange = (newPage) => {
        setPagination(prev => ({ ...prev, page: newPage }));
    };

    const handleSortChange = (newSortBy, newSortDescending) => {
        setSortConfig({ sortBy: newSortBy, sortDescending: newSortDescending });
        setPagination(prev => ({ ...prev, page: 1 }));
    };

    return (
        <ThemeProvider>
            <ErrorBoundary>
                <Container className="py-5">
                    <div className="d-flex justify-content-between align-items-center mb-4">
                        <h1>Comments System</h1>
                        <ThemeToggle />
                    </div>

                    {error && (
                        <Alert variant="danger" dismissible onClose={() => setError(null)}>
                            {error}
                        </Alert>
                    )}

                    <CommentForm onCommentAdded={handleCommentAdded} />

                    <div className="mt-5">
                        <CommentList
                            comments={comments}
                            loading={loading}
                            pagination={pagination}
                            sortConfig={sortConfig}
                            onPageChange={handlePageChange}
                            onSortChange={handleSortChange}
                            onCommentAdded={handleCommentAdded}
                            onCommentDeleted={handleCommentDeleted}
                        />
                    </div>
                </Container>
            </ErrorBoundary>
        </ThemeProvider>
    );
}

export default App;