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
