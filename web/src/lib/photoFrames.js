// Photo/video frames per asset family (BACKEND_SPEC §16.4) — mirrors the backend's validation.
export const PhotoFrames = {
  property: {
    photos: ["Entrance with Customer", "Selfie with Property", "Approach Road", "Site Plan", "Map Image", "Electric Meter", "Electricity Bill", "Building Elevation"],
    videos: ["Property Walkthrough", "Site Approach Drive", "Interior 360° Pan"],
  },
  plot: {
    photos: ["Plot — Front View", "Plot — Rear View", "Approach Road", "Boundary / Corner Peg", "Layout / Site Plan", "Map / Satellite", "Adjacent Landmark", "Surveyor at Site"],
    videos: ["Plot Walkthrough", "Approach Road Drive", "Boundary Pan"],
  },
  agri: {
    photos: ["Cultivated Parcel", "Irrigation Source", "Approach Track", "Boundary Bund", "Khasra Map", "Map / Satellite", "Power Connection", "Surveyor at Site"],
    videos: ["Parcel Walkthrough", "Approach Track Drive", "Irrigation Source Clip"],
  },
};
