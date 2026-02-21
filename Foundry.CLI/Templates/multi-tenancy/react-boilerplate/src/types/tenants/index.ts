import type { UserDto } from "../Users";

export interface Tenant {
  id: string;
  name: string;
  address: address;
  users: UserDto[];
}

export interface address {
  street: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  number: string;
}
