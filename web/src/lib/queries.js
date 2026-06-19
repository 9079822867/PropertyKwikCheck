import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import api from "./api.js";

// ---- leads ------------------------------------------------------------------
export function useLeads(bucket, q) {
  return useQuery({
    queryKey: ["leads", bucket, q ?? ""],
    queryFn: async () => {
      const params = { bucket };
      if (q) params.q = q;
      const { data } = await api.get("/leads", { params });
      return data; // { rows, total, counts }
    },
    placeholderData: (prev) => prev, // keep previous page while refetching (v5)
  });
}

export function useLead(id) {
  return useQuery({
    queryKey: ["lead", id],
    queryFn: async () => (await api.get(`/leads/${id}`)).data,
    enabled: id != null,
  });
}

export function useUpdateLead(id) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body) => (await api.patch(`/leads/${id}`, body)).data,
    onSuccess: (lead) => {
      qc.setQueryData(["lead", id], lead);
      qc.invalidateQueries({ queryKey: ["leads"] });
      qc.invalidateQueries({ queryKey: ["meta"] });
    },
  });
}

// Generic per-row lead action ({ id, body }) for list views (reassign/reject/delete).
export function useLeadAction() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body, method }) =>
      (method === "delete" ? await api.delete(`/leads/${id}`) : await api.patch(`/leads/${id}`, body)).data,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["leads"] });
      qc.invalidateQueries({ queryKey: ["meta"] });
    },
  });
}

export function useCreateLead() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body) => (await api.post("/leads", body)).data, // { ptype, data }
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["leads"] });
      qc.invalidateQueries({ queryKey: ["meta"] });
    },
  });
}

// ---- directory: users -------------------------------------------------------
export function useUsers() {
  return useQuery({ queryKey: ["users"], queryFn: async () => (await api.get("/users")).data });
}
export function useRoles() {
  return useQuery({ queryKey: ["roles"], queryFn: async () => (await api.get("/roles")).data, staleTime: Infinity });
}
export function useUserTypes() {
  return useQuery({ queryKey: ["usertypes"], queryFn: async () => (await api.get("/usertypes")).data, staleTime: Infinity });
}
export function useCreateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body) => (await api.post("/users", body)).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}
export function useUpdateUser(id) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body) => (await api.patch(`/users/${id}`, body)).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}
export function useDeleteUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id) => (await api.delete(`/users/${id}`)).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["users"] }),
  });
}

// ---- directory: companies ---------------------------------------------------
export function useCompanies() {
  return useQuery({ queryKey: ["companies"], queryFn: async () => (await api.get("/companies")).data });
}
export function useCreateCompany() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body) => (await api.post("/companies", body)).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["companies"] }),
  });
}
export function useUpdateCompany(id) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body) => (await api.patch(`/companies/${id}`, body)).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["companies"] }),
  });
}

// ---- dashboard / meta -------------------------------------------------------
export function useMeta() {
  return useQuery({
    queryKey: ["meta"],
    queryFn: async () => (await api.get("/meta")).data, // { bucketCounts }
    staleTime: 30_000,
  });
}

export function useAnalytics() {
  return useQuery({
    queryKey: ["analytics"],
    queryFn: async () => (await api.get("/analytics")).data,
    staleTime: 5 * 60_000,
  });
}

export function useScreen(name) {
  return useQuery({
    queryKey: ["screen", name],
    queryFn: async () => (await api.get(`/screens/${name}`)).data,
  });
}

// ---- master data CRUD -------------------------------------------------------
export function useMasterItems(category, enabled = true) {
  return useQuery({
    queryKey: ["master", category],
    queryFn: async () => (await api.get(`/master/${category}`)).data, // [{ id, value }]
    enabled,
    staleTime: 5 * 60_000,
  });
}

export function useAddMasterItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ category, value }) => (await api.post("/master", { category, value })).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["screen", "master"] }),
  });
}

// ---- photos (wizard stage 5) ------------------------------------------------
export function usePhotos(leadId) {
  return useQuery({
    queryKey: ["photos", leadId],
    queryFn: async () => (await api.get(`/leads/${leadId}/photos`)).data,
    enabled: leadId != null,
  });
}

export function useUploadPhoto(leadId) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ file, frameLabel, kind }) => {
      const form = new FormData();
      form.append("file", file);
      form.append("frameLabel", frameLabel);
      form.append("kind", kind || "photo");
      // Let axios set the multipart boundary (override the default JSON content-type).
      const { data } = await api.post(`/leads/${leadId}/photos`, form, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      return data;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["photos", leadId] }),
  });
}

export function useDeletePhoto(leadId) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (photoId) => (await api.delete(`/photos/${photoId}`)).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["photos", leadId] }),
  });
}
