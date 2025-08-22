import ProtectedRoutes from "./ProtectedRoute";
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate } from "react-router-dom";
import { Dashboard, ThemeProvider, SearchWidget, UserDropdown } from 'leximo-dashboard';
import { AuthProvider, useAuth, SplitLogin } from 'leximo-auth';
import 'leximo-dashboard/dist/assets/style.css';
import 'leximo-auth/dist/assets/leximo-auth.css';

export default function App() {
    return (
        <AuthProvider>
            <ThemeProvider>
                <Router>
                    <AppContent />
                </Router>
            </ThemeProvider>
        </AuthProvider>
    );
}

function AppContent() {
    const { user, logout } = useAuth();
    const navigate = useNavigate();
    const menuItems = [
        {
            label: 'Home',
            icon: 'home',
            roles: ['admin', 'user'],
            action: ({ navigate }) => navigate('/')
        },
        //{
        //    label: 'Logout',
        //    icon: 'sign-out-alt',
        //    action: ({ navigate }) => {
        //        logout();
        //        navigate('/login');
        //    }
        //},
        //{
        //    label: 'Admin',
        //    icon: 'lock',
        //    subItems: [
        //        {
        //            label: 'Users',
        //            action: ({ navigate }) => navigate('/admin/users')
        //        },
        //        {
        //            label: 'System Settings',
        //            action: ({ navigate, closeMenu }) => {
        //                closeMenu();
        //                openAdvancedSettings();
        //            }
        //        }
        //    ]
        //}
    ];

    const topBarConfig = {
        leftContent: null,
        centerContent: (
            <SearchWidget
                placeholder="Find anything..."
                onSearch={(term) => console.log(term)}
            />
        ),
        rightContent: (
            <UserDropdown
                items={[
                    { label: 'Profile', icon: 'user' },
                    { label: 'Notifications', icon: 'bell' },
                    {
                        label: 'Logout',
                        icon: 'sign-out-alt',
                        onClick: () => {
                            logout();
                            navigate('/login');
                        }
                    }
                ]}
            />
        ),
        showThemeToggle: true
    };

    return (
        <>
            <Routes>
                <Route element={<ProtectedRoutes />}>
                    <Route path="/" element={<Dashboard menuItems={menuItems} topBarConfig={topBarConfig}  />} />
                </Route>
                <Route path="/login" element={<SplitLogin endpoints={{
                    login: "/api/v1/auth/login",
                    logout: "/api/v1/auth/logout",
                    me: "/api/v1/auth/me",
                    refresh: "/api/v1/auth/refresh"
                }}
                    tokenStorageKey="my-app-auth-token"
                />} />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </>
    );
}

