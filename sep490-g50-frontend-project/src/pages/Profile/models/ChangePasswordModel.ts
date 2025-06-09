import { z } from 'zod';

// Define the account schema
export const changePasswordSchema = z.object({
  oldPassword: z.string(),
  newPassword: z.string()
    .min(8, "Password must be at least 7 characters")
    .max(32, "Password must be at most 32 characters")
    .regex(/[A-Z]/, "Password must contain at least one uppercase letter")
    .regex(/\d/, "Password must contain at least one number")
    .regex(/[!@#$%^&*(),.?":{}|<>]/, { message: "Password must contain at least one special character" }),
  confirmPassword: z.string(),
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: "Passwords do not match",
  path: ["confirmPassword"], // Error will appear on confirmPassword field
});

// Create TypeScript type from the Zod schema
export type ChangePasswordPayload = z.infer<typeof changePasswordSchema>;