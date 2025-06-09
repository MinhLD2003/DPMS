import { z } from 'zod';

// Define the account schema
export const loginSchema = z.object({
    email: z.string().email({ message: "Invalid email address" }),
    password: z.string().min(8).max(32)
     .regex(/[A-Z]/, "Password must contain at least one uppercase letter")
     .regex(/\d/, "Password must contain at least one number")
     .regex(/[!@#$%^&*(),.?":{}|<>]/, { message: "Password must contain at least one special character" }),
});

// Create TypeScript type from the Zod schema
export type LoginPayload = z.infer<typeof loginSchema>;

