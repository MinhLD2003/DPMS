import { z } from 'zod';

// Define the group schema
export const systemSchema = z.object({
    name: z.string().min(4, { message: "Name must be longer." }),
    domain: z.string().url({message: "Enter a correct domain url."}).min(4, { message: "Domain must be longer." }),
    description: z.string().min(6, { message: "Description must be longer" }),
    productDevEmails: z.array(z.string()),
    businessOwnerEmails: z.array(z.string()),
});
// Create TypeScript type from the Zod schema
export type PostSystemModel = z.infer<typeof systemSchema>;