export interface SystemNotificationDto {
    id: number;
    title: string;
    content: string;
    isRead: boolean;
    createdAt: Date;
}

export interface CreateNotificationDto {
    title: string;
    content: string;
    userIds: string[];
}
