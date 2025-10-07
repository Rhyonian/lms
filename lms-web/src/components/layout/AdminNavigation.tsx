import { NavLink, Outlet } from "react-router-dom";
import { cn } from "@/lib/utils";

const adminNav = [
  { to: "/admin/users", label: "Users" },
  { to: "/admin/groups", label: "Groups" },
  { to: "/admin/courses", label: "Courses" }
];

export function AdminNavigation() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Admin Control Center</h1>
        <p className="mt-2 text-muted-foreground">
          Manage learners, groups, and course assets across your learning ecosystem.
        </p>
      </div>
      <div className="flex items-center gap-4 border-b pb-2">
        {adminNav.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              cn(
                "pb-2 text-sm font-medium transition-colors",
                isActive ? "text-primary border-b-2 border-primary" : "text-muted-foreground hover:text-primary"
              )
            }
          >
            {item.label}
          </NavLink>
        ))}
      </div>
      <Outlet />
    </div>
  );
}
