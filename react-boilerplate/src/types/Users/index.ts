export interface UserDto {
  id: string;
  name: string;
  email?: string;
  roles?: string[];
}

export interface InviteData {
  id: string;
  email: string;
  status: 'pending' | 'accepted' | 'expired' | 'cancelled';
  sendedAt: Date;
  expirationTime: Date;
  acceptedAt?: Date;
}

export interface AcceptInvitationData {
  token: string;
  email: string;
  tenant: string;
  tenantId?: string;
  name: string;
  password: string;
  confirmPassword: string;
}
