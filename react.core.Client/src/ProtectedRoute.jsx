import { useAuth } from "leximo-auth";
import { Navigate, Outlet } from 'react-router-dom'

export default function ProtectedRoutes() {
    const { user, loading } = useAuth();

    if (loading) {
        return (
            <div className="d-flex justify-content-center align-items-center vh-100">
                <i className="fas fa-spinner fa-spin fa-3x text-primary"></i>
            </div>
        );
    }

    return user ? <Outlet /> : <Navigate to="/login" replace />;
}