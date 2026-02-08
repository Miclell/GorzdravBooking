import { createContext, useContext, useState, useEffect } from 'react';
import * as api from '../services/api';

const AuthContext = createContext();

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
};

export const AuthProvider = ({ children }) => {
  const [userId, setUserId] = useState(null);
  const [username, setUsername] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const restoreSession = async () => {
      try {
        const response = await api.getMe();
        const data = typeof response === 'string' ? JSON.parse(response) : response;
        const { id, username: apiUsername } = data;
        localStorage.setItem('userId', id);
        localStorage.setItem('username', apiUsername);
        setUserId(id);
        setUsername(apiUsername);
      } catch {
        localStorage.removeItem('userId');
        localStorage.removeItem('username');
        setUserId(null);
        setUsername(null);
      } finally {
        setLoading(false);
      }
    };
    restoreSession();
  }, []);

  const login = async (usernameInput, password) => {
    try {
      let response = await api.loginUser(usernameInput, password);
      if (typeof response === 'string') response = JSON.parse(response);
      const { id, username: apiUsername } = response;
      localStorage.setItem('userId', id);
      localStorage.setItem('username', apiUsername);
      setUserId(id);
      setUsername(apiUsername);
      return { success: true, userId: id, username: apiUsername };
    } catch (error) {
      return { success: false, error: error.message };
    }
  };

  const register = async (usernameInput, password) => {
    try {
      let response = await api.registerUser(usernameInput, password);
      if (typeof response === 'string') response = JSON.parse(response);
      const { id, username: apiUsername } = response;
      localStorage.setItem('userId', id);
      localStorage.setItem('username', apiUsername);
      setUserId(id);
      setUsername(apiUsername);
      return { success: true, userId: id, username: apiUsername };
    } catch (error) {
      return { success: false, error: error.message };
    }
  };

  const logout = async () => {
    try {
      await api.logoutApi();
    } finally {
      localStorage.removeItem('userId');
      localStorage.removeItem('username');
      setUserId(null);
      setUsername(null);
    }
  };

  return (
    <AuthContext.Provider
      value={{ userId, username, login, register, logout, loading }}
    >
      {children}
    </AuthContext.Provider>
  );
};
