import { Navigate, Outlet, RouteObject, useLocation } from "react-router-dom";
import Login from "./pages/Login";
import { useAuth } from "./lib/auth";

export const ProtectedRoute = () => {
  const { user, loading } = useAuth();
  const location = useLocation();

  if (loading) {
    return <div className="flex h-full items-center justify-center">Loading...</div>;
  }

  if (!user) {
    const fullPath = `${location.pathname}${location.search}${location.hash}` || "/";
    const search = new URLSearchParams({ next: fullPath }).toString();
    return <Navigate to={`/login?${search}`} replace />;
  }

  return <Outlet />;
};

export const routes: RouteObject[] = [
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: "/",
        element: <Outlet />,
      },
    ],
  },
  {
    path: "/login",
    element: <Login />,
  },
  {
    path: "*",
    element: <Navigate to="/" replace />,
  },
];

export default routes;
