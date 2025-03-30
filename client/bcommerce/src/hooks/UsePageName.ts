import { useLocation } from "react-router-dom";
import { useMemo } from "react";

const UsePageName = () => {
  const location = useLocation();

  const pageName = useMemo(() => {
    const pathSegments = location.pathname
      .split("/")
      .filter((segment) => segment.length > 0);

    return pathSegments[pathSegments.length - 1] || "home"; // fallback opcional
  }, [location.pathname]);

  return pageName;
};

export default UsePageName;
