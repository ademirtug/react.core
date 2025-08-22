/* eslint-disable react-refresh/only-export-components */
import { createContext, useState, useEffect, useContext } from "react";
import { jwtDecode } from 'jwt-decode';

const AuthContext = createContext(null);
export const useAuth = () => useContext(AuthContext);

export default function AuthProvider({
    children,
    endpoints = {
        login: "/api/v1/auth/login",
        logout: "/api/v1/auth/logout",
        me: "/api/v1/auth/me",
        refresh: "/api/v1/auth/refresh"
    },
    tokenStorageKey = "react-auth-token"
}) {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);
    const [token, setToken] = useState(
        localStorage.getItem(tokenStorageKey) ||
        sessionStorage.getItem(tokenStorageKey) ||
        null
    );

    useEffect(() => {
        const fetchUser = async () => {
            const token = localStorage.getItem(tokenStorageKey) || sessionStorage.getItem(tokenStorageKey);
            if (!token) {
                setLoading(false);
                return;
            }

            try {
                const response = await fetch(endpoints.me, {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                });

                if (response.ok) {
                    const result = await response.json();
                    setUser(result);
                    setToken(token); // Ensure token is set in state
                } else {
                    // If the token is invalid, clear it from both storage locations
                    localStorage.removeItem(tokenStorageKey);
                    sessionStorage.removeItem(tokenStorageKey);
                    setToken(null);
                }
            } catch (e) {
                console.error("Auth check failed", e);
                localStorage.removeItem(tokenStorageKey);
                sessionStorage.removeItem(tokenStorageKey);
                setToken(null);
            } finally {
                setLoading(false);
            }
        };

        fetchUser();
    }, [endpoints.me, tokenStorageKey]);

    const login = async (email, password, rememberMe = false) => {
        try {
            const response = await fetch(endpoints.login, {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: new URLSearchParams({ email, password }).toString(),
            });

            if (!response.ok) {
                const errorData = await response.json();
                console.error("Login failed:", errorData.message);
                return false;
            }

            const data = await response.json();
            const decoded = jwtDecode(data.token);
            const userRole = decoded.role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

            const user = {
                email: data.user?.email ?? email,
                role: userRole
            };

            // Store token based on rememberMe flag
            if (rememberMe) {
                localStorage.setItem(tokenStorageKey, data.token);
            } else {
                sessionStorage.setItem(tokenStorageKey, data.token);
            }

            setToken(data.token);
            setUser(user);

            return true;

        } catch (error) {
            console.error("Login error:", error);
            return false;
        }
    };

    const logout = async () => {
        try {
            await fetch(endpoints.logout, {
                method: "POST",
                headers: {
                    Authorization: `Bearer ${token}`
                }
            });
        } catch (e) {
            console.error("Logout error:", e);
        } finally {
            localStorage.removeItem(tokenStorageKey);
            sessionStorage.removeItem(tokenStorageKey);
            setToken(null);
            setUser(null);
        }
    };

    const refreshToken = async () => {
        try {
            const response = await fetch(endpoints.refresh, {
                method: "POST",
                headers: {
                    Authorization: `Bearer ${token}`
                }
            });
            if (response.ok) {
                const data = await response.json();
                localStorage.setItem(tokenStorageKey, data.token);
                setToken(data.token);
                return true;
            }
        } catch (e) {
            console.error("Token refresh failed", e);
        }
        return false;
    };

    return (
        <AuthContext.Provider value={{
            user,
            token,
            login,
            logout,
            refreshToken,
            loading
        }}>
            {!loading ? children :
                <div className="d-flex justify-content-center align-items-center vh-100">
                    <i className="material-symbols-rounded text-primary">progress_activity</i>
                </div>
            }
        </AuthContext.Provider>
    );
}