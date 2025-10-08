// apps/web/src/App.tsx
import { Link, useRoutes } from "react-router-dom";
import routes from "./routes";

export default function App() {
  const element = useRoutes(routes);
  return (
    <div style={{ fontFamily: "system-ui, sans-serif", padding: 16 }}>
      <header style={{ display: "flex", gap: 12, alignItems: "center", marginBottom: 16 }}>
        <strong>LMS</strong>
        <Link to="/">Home</Link>
        <Link to="/catalog">Catalog</Link>
        <Link to="/admin">Admin</Link>
      </header>
      <main>{element}</main>
    </div>
  );
}
