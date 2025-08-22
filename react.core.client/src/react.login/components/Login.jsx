import React, { useState } from 'react';
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../providers/AuthProvider";
import './Login.css';

export default function SplitLogin() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [rememberMe, setRememberMe] = useState(false);
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const { login } = useAuth();

    const redirectTo = searchParams.get("redirect") || "";

    const handleLogin = async (e) => {
        e.preventDefault();
        setIsLoading(true);
        const result = await login(email, password, rememberMe);
        setIsLoading(false);
        if (result) navigate(redirectTo, { replace: true });
        else setError('Invalid email or password');
    };

    return (
        <div className="login-container">
            <div className="login-left">
                <div className="overlay" />
                <div className="content">
                    <h2>Welcome Back</h2>
                    <p>Login to access your dashboard and manage your account.</p>
                </div>
            </div>

            <div className="login-right">
                <div className="login-form-container">
                    <h2>Sign In</h2>
                    <p>Enter your details to login</p>

                    {error && <div className="login-error">{error}</div>}

                    <form className="login-form" onSubmit={handleLogin}>
                        <label>Email</label>
                        <input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder="Enter your email"
                        />

                        <label>Password</label>
                        <input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            placeholder="Enter your password"
                        />

                        <div className="login-remember">
                            <label>
                                <input type="checkbox" onClick={(e) => setRememberMe(e.target.checked)} /> Remember me
                            </label>
                            <a href="#">Forgot password?</a>
                        </div>

                        <button className="login-button" type="submit" disabled={isLoading}>
                            {isLoading ? <><span className="login-spinner"></span> Signing in...</> : 'Sign In'}
                        </button>
                    </form>

                    {/*<div className="login-social">*/}
                    {/*    <p>Or sign in with</p>*/}
                    {/*    <div className="login-social-icons">*/}
                    {/*        <button*/}
                    {/*            type="button"*/}
                    {/*            onClick={() => handleSocialLogin('google')}*/}
                    {/*            aria-label="Sign in with Google"*/}
                    {/*        >*/}
                    {/*            <i className="fab fa-google"></i>*/}
                    {/*        </button>*/}
                    {/*        <button*/}
                    {/*            type="button"*/}
                    {/*            onClick={() => handleSocialLogin('github')}*/}
                    {/*            aria-label="Sign in with GitHub"*/}
                    {/*        >*/}
                    {/*            <i className="fab fa-github"></i>*/}
                    {/*        </button>*/}
                    {/*        <button*/}
                    {/*            type="button"*/}
                    {/*            onClick={() => handleSocialLogin('microsoft')}*/}
                    {/*            aria-label="Sign in with Microsoft"*/}
                    {/*        >*/}
                    {/*            <i className="fab fa-microsoft"></i>*/}
                    {/*        </button>*/}
                    {/*    </div>*/}
                    {/*</div>*/}

                    {/*<p className="login-signup">*/}
                    {/*    Don't have an account? <a href="#">Sign up</a>*/}
                    {/*</p>*/}
                </div>
            </div>
        </div>
    );
}
