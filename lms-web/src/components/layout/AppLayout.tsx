import { NavLink, Outlet } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const navItems = [
  { to: "/catalog", label: "Catalog" },
  { to: "/admin/users", label: "Admin" },
  { to: "/learn/demo", label: "Learn" }
];

export function AppLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      <header className="border-b bg-background">
        <div className="container flex h-16 items-center justify-between gap-6">
          <NavLink to="/" className="text-xl font-semibold">
            lms-web
          </NavLink>
          <nav className="flex items-center gap-6">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                className={({ isActive }) =>
                  cn(
                    "text-sm font-medium transition-colors hover:text-primary",
                    isActive ? "text-primary" : "text-muted-foreground"
                  )
                }
              >
                {item.label}
              </NavLink>
            ))}
          </nav>
          <div className="flex items-center gap-3">
            <Button variant="ghost" className="text-muted-foreground" disabled>
              OIDC Sign In
            </Button>
            <Button variant="outline" disabled>
              OIDC Profile
            </Button>
          </div>
        </div>
      </header>
      <main className="flex-1 bg-muted/30">
        <div className="container py-8">
          <Outlet />
        </div>
      </main>
      <footer className="border-t bg-background">
        <div className="container py-4 text-sm text-muted-foreground">
          Placeholder footer • OIDC tenant status: <span className="font-medium">Not configured</span>
        </div>
      </footer>
    </div>
  );
}
