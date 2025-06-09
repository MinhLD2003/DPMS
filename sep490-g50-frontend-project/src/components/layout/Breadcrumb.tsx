import { Breadcrumb } from "antd";
import { Link, useLocation } from "react-router-dom";

const breadcrumbMap = {
  dashboard: "Dashboard",
  features: "Feature Management",
  groups: "Group Management",
  accounts: "Account Management",
  systems: "System Management",
  new: "New",
};

const Breadcrumbs = () => {
  const location = useLocation();
  const pathnames = location.pathname.split("/").filter((x) => x);

  const breadcrumbItems = [
    {
    },
    ...pathnames.map((value, index) => ({
      title: (
        <Link to={`/${pathnames.slice(0, index + 1).join("/")}`}>
          {breadcrumbMap[value as keyof typeof breadcrumbMap] || value}
        </Link>
      ),
      key: `/${pathnames.slice(0, index + 1).join("/")}`,
    })),
  ];

  return <Breadcrumb style={{ marginBottom: 16 }} items={breadcrumbItems} />;
};

export default Breadcrumbs;
  