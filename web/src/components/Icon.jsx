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
  reassign: "M3 7h13l-3-3M21 17H8l3 3",
  alert: "M12 3l9 16H3zM12 10v4M12 17h.01",
  rupee: "M7 5h10M7 9h10M14 5c2 0 4 2 4 5s-2 5-5 5H8l6 4",
  reject: "M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18ZM8 8l8 8",
  bank: "M3 9l9-5 9 5M4 10v8M9 10v8M15 10v8M20 10v8M3 21h18",
  map: "M9 3 3 5v16l6-2 6 2 6-2V3l-6 2-6-2Zm0 0v16m6-14v16",
  view: "M2 12s4-7 10-7 10 7 10 7-4 7-10 7S2 12 2 12Zm10 3a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z",
  pin: "M12 21s7-6 7-11a7 7 0 1 0-14 0c0 5 7 11 7 11Zm0-8a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z",
  building: "M4 21V5a1 1 0 0 1 1-1h6a1 1 0 0 1 1 1v16M12 21V9h7a1 1 0 0 1 1 1v11M7 8h2M7 12h2M16 13h1M16 17h1",
  check: "M20 6 9 17l-5-5",
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
