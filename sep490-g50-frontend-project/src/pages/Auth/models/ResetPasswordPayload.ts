import { z } from "zod";

export const resetPasswordSchema = z.object({
    new_password: z.string()
        .min(8, { message: "Password must be at least 8 characters" })
        .max(32, { message: "Password cannot exceed 32 characters" })
        .regex(/[A-Z]/, "Password must contain at least one uppercase letter")
        .regex(/\d/, "Password must contain at least one number")
        .regex(/[!@#$%^&*(),.?":{}|<>]/, { message: "Password must contain at least one special character" }),


    confirm_password: z.string(),
}).refine((data) => data.new_password === data.confirm_password, {
    message: "The two passwords do not match!",
    path: ["confirm_password"],
});

export type ResetPasswordPayload = z.infer<typeof resetPasswordSchema>;