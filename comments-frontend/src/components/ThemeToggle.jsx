// File: src/components/ThemeToggle.jsx
import React, { useContext } from 'react';
import { Button } from 'react-bootstrap';
import { ThemeContext } from './ThemeContext';

const ThemeToggle = () => {
    const { theme, toggleTheme } = useContext(ThemeContext);

    return (
        <Button variant="outline-secondary" onClick={toggleTheme}>
            {theme === 'light' ? 'Switch to Dark Mode' : 'Switch to Light Mode'}
        </Button>
    );
};

export default ThemeToggle;