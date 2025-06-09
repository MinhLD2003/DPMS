import { z } from 'zod';

// Define the group schema
export const groupSchema = z.object({
    name: z.string().min(1, { message: "Name is required" }).max(50, { message: "Name must be at most 50 characters long" }),
    description: z.string().min(1, { message: "Description is required" }).max(200, { message: "Description must be at most 200 characters long" }),
});

// Create TypeScript type from the Zod schema
export type PostGroupModel = z.infer<typeof groupSchema>;