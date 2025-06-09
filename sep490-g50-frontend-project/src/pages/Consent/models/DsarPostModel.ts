import { z } from "zod";

export const DSARSchema = z.object({
    requesterName: z.string().min(1, "Full name is required"),
    requesterEmail: z.string().email("Invalid email address"),
    phoneNumber: z.string().optional(),
    address: z.string().optional(),
    description: z.string(),
    type: z.number(),
});

export type DSARRequest = z.infer<typeof DSARSchema>;
