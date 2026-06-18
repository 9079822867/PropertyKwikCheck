// Minimal inline-SVG icon set (stroke-based, matches the prototype's look).
const PATHS = {
  dashboard: "M3 13h8V3H3v10Zm0 8h8v-6H3v6Zm10 0h8V11h-8v10Zm0-18v6h8V3h-8Z",
  leads: "M4 6h16M4 12h16M4 18h16",
  folderadd: "M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V7Zm9 4v4m-2-2h4",
  user: "M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8Zm-7 8a7 7 0 0 1 14 0",
  company: "M3 21h18M5 21V7l8-4v18M19 21V11l-6-3",
  billing: "M4 4h16v16H4zM8 9h8M8 13h8M8 17h5",
  search: "M11 19a8 8 0 1 0 0-16 8 8 0 0 0 0 16Zm10 2-4.3-4.3",
  logout: "M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4m7 14 5-5-5-5m5 5H9",
  plus: "M12 5v14M5 12h14",
  chevron: "m9 18 6-6-6-6",
  shield: "M12 3l8 3v6c0 5-3.4 8-8 9-4.6-1-8-4-8-9V6l8-3Z",
  doc: "M7 3h7l5 5v13H7zM14 3v5h5",
  layers: "m12 3 9 5-9 5-9-5 9-5Zm9 11-9 5-9-5",
  clock: "M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18Zm0-14v5l3 2",
  home: "M3 11l9-8 9 8M5 9v11h14V9",
  trend: "M3 17l6-6 4 4 8-8M21 7v5h-5",
  buckets: "M3 7h18M3 12h18M3 17h18",
  master: "M4 6a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2zM4 10h16",
};

export default function Icon({ name, className, size = 18 }) {
  const d = PATHS[name] || PATHS.leads;
  return (
    <svg
      className={className}
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d={d} />
    </svg>
  );
}
