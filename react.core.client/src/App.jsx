import ProtectedRoute from "./ProtectedRoute";
import { BrowserRouter as Router, Routes, Route, Navigate, Outlet, useNavigate } from "react-router-dom";
import { Dashboard, ThemeProvider, SearchWidget, UserDropdown } from '@selestra11/react.dashboard';
//import SplitLogin from './react.login/components/Login';
//import AuthProvider, { useAuth } from './react.login/providers/AuthProvider';
import { AuthProvider, useAuth, SplitLogin } from '@selestra11/react.login';


import '@selestra11/react.dashboard/dist/assets/style.css';
import '@selestra11/react.login/dist/assets/react.login.css';

function Home() {
    return (
        <p>Hello world!</p>
    );
}

function SubMenuRoute() {
    return (
        <p>Submenu Route</p>
    );
}

function AdminLayout() {
    const { logout } = useAuth();
    const navigate = useNavigate();
    const menuItems = [
        {
            id: 'home',
            label: 'Home',
            icon: 'home',
            roles: ['admin', 'user'],
            action: ({ navigate }) => navigate('')
        },
        {
            id: 'dropdown_menu',
            label: 'Dropdown Menu',
            icon: 'language',
            subItems: [
                {
                    label: 'Sub Menu Route',
                    icon: 'description',
                    action: ({ navigate }) => navigate('/submenuroute')
                }
            ]
        }
    ];

    const topBarConfig = {
        leftContent: (
            <SearchWidget
                placeholder="Find anything..."
                onSearch={(term) => console.log(term)}
            />
        ),
        centerContent: null,
        rightContent: (
            <UserDropdown
                items={[
                    {
                        label: 'Logout',
                        icon: 'logout',
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
        <Dashboard menuItems={menuItems} topBarConfig={topBarConfig} title="Duo Word">
            <Outlet />
        </Dashboard>
    );
}


export default function App() {
    return (
        <AuthProvider>
            <ThemeProvider>
                <Router>
                    <Routes>
                        {/* Admin Routes */}
                        {/* Public Admin Routes */}
                        <Route path="/login" element={<SplitLogin
                            endpoints={{
                                login: "/api/v1/auth/login",
                                logout: "/api/v1/auth/logout",
                                me: "/api/v1/auth/me",
                                refresh: "/api/v1/auth/refresh"
                            }}
                            tokenStorageKey="my-app-auth-token"
                        />} />

                        {/* Protected Admin Routes */}
                        <Route path="" element={<ProtectedRoute />}>
                            <Route element={<AdminLayout />}>
                                <Route index element={<Home />} />
                                <Route path="/submenuroute" element={<SubMenuRoute />} />
                            </Route>
                        </Route>

                        {/* Catch-all */}
                        <Route path="*" element={<Navigate to="/" replace />} />
                    </Routes>
                </Router>
            </ThemeProvider>
        </AuthProvider>
    );
}

