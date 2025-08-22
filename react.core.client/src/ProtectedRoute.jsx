import { useAuth } from "@selestra11/react.login";
//import AuthProvider, { useAuth } from './react.login/providers/AuthProvider';
import { Navigate, Outlet, useLocation } from 'react-router-dom'


export default function ProtectedRoute() {
    const { user, loading } = useAuth();
    const location = useLocation();

    if (loading) {
        return (
            <div className="d-flex justify-content-center align-items-center vh-100">
                <i className="material-symbols-rounded fa-3x text-primary">progress_activity</i>
            </div>
        );
    }

    if (!user) {
        return <Navigate to={`/login?redirect=${encodeURIComponent(location.pathname)}`} replace />;
    }

    return <Outlet />;
}