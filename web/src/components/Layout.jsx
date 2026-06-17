import { Outlet } from "react-router-dom";
import Topbar from "./Topbar.jsx";
import Sidebar from "./Sidebar.jsx";

export default function Layout() {
  return (
    <>
      <Topbar />
      <Sidebar />
      <main>
        <Outlet />
      </main>
    </>
  );
}
