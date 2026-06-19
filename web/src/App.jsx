import { Routes, Route, Navigate } from "react-router-dom";
import Layout from "./components/Layout.jsx";
import ProtectedRoute from "./components/ProtectedRoute.jsx";
import Login from "./pages/Login.jsx";
import Dashboard from "./pages/Dashboard.jsx";
import Leads from "./pages/Leads.jsx";
import NewLead from "./pages/NewLead.jsx";
import LeadDetail from "./pages/LeadDetail.jsx";
import LeadWizard from "./pages/LeadWizard.jsx";
import Users from "./pages/Users.jsx";
import CreateUser from "./pages/CreateUser.jsx";
import Companies from "./pages/Companies.jsx";
import CreateCompany from "./pages/CreateCompany.jsx";
import ScreenPage from "./pages/ScreenPage.jsx";
import Analytics from "./pages/Analytics.jsx";
import Settings from "./pages/Settings.jsx";
import ReportView from "./pages/ReportView.jsx";
import NotFound from "./pages/NotFound.jsx";

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      {/* Public — shareable inspection report, viewable with or without login. */}
      <Route path="/report/:id" element={<ReportView />} />
      <Route
        element={
          <ProtectedRoute>
            <Layout />
          </ProtectedRoute>
        }
      >
        <Route index element={<Dashboard />} />
        <Route path="leads" element={<Leads />} />
        <Route path="leads/new" element={<NewLead />} />
        <Route path="leads/:id" element={<LeadDetail />} />
        <Route path="leads/:id/edit" element={<LeadWizard />} />
        <Route path="users" element={<Users />} />
        <Route path="users/new" element={<CreateUser />} />
        <Route path="users/:id/edit" element={<CreateUser />} />
        <Route path="companies" element={<Companies />} />
        <Route path="companies/new" element={<CreateCompany />} />
        <Route path="companies/:id/edit" element={<CreateCompany />} />
        <Route path="screens/:name" element={<ScreenPage />} />
        <Route path="analytics" element={<Analytics />} />
        <Route path="settings" element={<Settings />} />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  );
}
