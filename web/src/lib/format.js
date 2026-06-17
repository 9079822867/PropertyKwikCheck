// Indian digit grouping for rupees (BACKEND_SPEC §11): 21540000 -> "₹ 2,15,40,000".
export function inr(amount) {
  if (amount == null || amount === "") return "—";
  const n = Number(amount);
  if (Number.isNaN(n)) return "—";
  return "₹ " + n.toLocaleString("en-IN");
}

export function initials(name) {
  if (!name) return "?";
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((w) => w[0])
    .join("")
    .toUpperCase();
}
