import { z } from "zod";
import { UserStatus } from "../../../enum/enum";

export type UserProfile = {
    email: string;
    groups: string[];
    systems: string[];
    fullName: string;
    userName: string;
    dob: string;
    address: string;
    phone: string;
}

export type UserAccount = {
    id: string;
    status: UserStatus;
    createdAt: string;
    createdBy: string;
    lastModifiedAt: string;
    lastModifiedBy: string;
    lastTimeLogin: string;
}
// Define the group schema
export const profileSchema = z.object({
    email: z.string().email("Please enter a valid email"),
    fullName: z.string().min(1, { message: "Full name is required" }).max(50, { message: "Full name must be at most 50 characters long" }),
    userName: z.string().min(1, { message: "Username is required" }).max(50, { message: "Username must be at most 50 characters long" }),
    dob: z.coerce.date().refine(
        (date) => date < new Date(),
        "Date of birth must be in the past"
      ),
});

// Create TypeScript type from the Zod schema
export type ProfilePostModel = z.infer<typeof profileSchema>;

export type UserModel = UserAccount & UserProfile;
