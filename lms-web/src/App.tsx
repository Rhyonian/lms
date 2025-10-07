import { Navigate, Route, Routes } from "react-router-dom";
import { AppLayout } from "@/components/layout/AppLayout";
import { AdminNavigation } from "@/components/layout/AdminNavigation";
import CatalogPage from "@/pages/CatalogPage";
import CoursePage from "@/pages/CoursePage";
import LearnPage from "@/pages/LearnPage";
import AdminUsersPage from "@/pages/AdminUsersPage";
import AdminGroupsPage from "@/pages/AdminGroupsPage";
import AdminCoursesPage from "@/pages/AdminCoursesPage";
import OidcSignInPage from "@/pages/OidcSignInPage";
import OidcCallbackPage from "@/pages/OidcCallbackPage";
import NotFoundPage from "@/pages/NotFoundPage";

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<AppLayout />}>
        <Route index element={<Navigate to="/catalog" replace />} />
        <Route path="catalog" element={<CatalogPage />} />
        <Route path="course/:courseId" element={<CoursePage />} />
        <Route path="learn/:courseId">
          <Route index element={<LearnPage />} />
          <Route path=":lessonId" element={<LearnPage />} />
        </Route>
        <Route path="admin" element={<AdminNavigation />}>
          <Route index element={<Navigate to="users" replace />} />
          <Route path="users" element={<AdminUsersPage />} />
          <Route path="groups" element={<AdminGroupsPage />} />
          <Route path="courses" element={<AdminCoursesPage />} />
        </Route>
        <Route path="oidc">
          <Route path="sign-in" element={<OidcSignInPage />} />
          <Route path="callback" element={<OidcCallbackPage />} />
        </Route>
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
