import { z } from 'zod';

// Define the account schema
export const accountSchema = z.object({
  fullName: z.string().min(6, { message: "Full name must be 6-50 characters long" }).max(50, { message: "Full name must be 6-50 characters long" }),
  email: z.string().email({ message: "Invalid email address" }),
  username: z.string().min(5, { message: "Username must be 5-50 characters long" }).max(50, { message: "Username must be 5-50 characters long" }),
  password: z.string()
    .transform(val => val === "" ? undefined : val)
    .optional()
    .refine(val => val === undefined || val.length >= 8, { message: "Password must be at least 8 characters" })
    .refine(val => val === undefined || val.length <= 32, { message: "Password cannot exceed 32 characters" })
    .refine(val => val === undefined || /[A-Z]/.test(val), { message: "Password must contain at least one uppercase letter" })
    .refine(val => val === undefined || /\d/.test(val), { message: "Password must contain at least one number" })
    .refine(val => val === undefined || /[!@#$%^&*(),.?":{}|<>]/.test(val), { message: "Password must contain at least one special character" }),
  status: z.nativeEnum({ Active: 0, Inactive: 1 }),
});

// Create TypeScript type from the Zod schema
export type PostAccountModel = z.infer<typeof accountSchema>;