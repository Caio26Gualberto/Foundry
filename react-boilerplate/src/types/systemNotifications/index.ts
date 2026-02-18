export interface SystemNotificationDto {
    id: number;
    title: string;
    content: string;
    isRead: boolean;
    createdAt: Date;
}

export interface TenantNotificationDto {
    id: string;
    title: string;
    content: string;
    createdAt: Date;
    usersCount: number;
}

export interface CreateNotificationDto {
    title: string;
    content: string;
    userIds: string[];
}
