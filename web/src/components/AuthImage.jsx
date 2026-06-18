import { useEffect, useState } from "react";
import api from "../lib/api.js";

// Loads an auth-protected image (download endpoint needs the bearer token,
// which a plain <img src> can't send) and renders it via an object URL.
export default function AuthImage({ path, alt, style }) {
  const [url, setUrl] = useState(null);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    let revoked = false;
    let objectUrl;
    api
      .get(path, { responseType: "blob" })
      .then((res) => {
        if (revoked) return;
        objectUrl = URL.createObjectURL(res.data);
        setUrl(objectUrl);
      })
      .catch(() => setFailed(true));
    return () => {
      revoked = true;
      if (objectUrl) URL.revokeObjectURL(objectUrl);
    };
  }, [path]);

  if (failed) return <div style={{ ...style, display: "grid", placeItems: "center", background: "var(--slate-100)", color: "var(--slate-400)", fontSize: 11 }}>n/a</div>;
  if (!url) return <div style={{ ...style, background: "var(--slate-100)" }} />;
  return <img src={url} alt={alt} style={style} />;
}
