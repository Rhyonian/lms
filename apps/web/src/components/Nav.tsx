import { Link } from "react-router-dom";
import { useAuth } from "../lib/auth";

const Nav = () => {
  const { user, logout } = useAuth();

  return (
    <nav className="flex items-center justify-between border-b border-gray-200 bg-white px-6 py-4 shadow-sm">
      <Link to="/" className="text-lg font-semibold text-gray-900">
        LMS
      </Link>
      <div className="flex items-center gap-4 text-sm">
        {user ? (
          <>
            <span className="text-gray-700">{user.email}</span>
            <button
              onClick={logout}
              className="rounded-md border border-transparent bg-gray-100 px-3 py-1 text-sm font-medium text-gray-700 transition hover:bg-gray-200"
            >
              Logout
            </button>
          </>
        ) : (
          <Link
            to="/login"
            className="rounded-md bg-indigo-600 px-3 py-1 text-sm font-medium text-white transition hover:bg-indigo-700"
          >
            Login
          </Link>
        )}
      </div>
    </nav>
  );
};

export default Nav;
