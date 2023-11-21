import { Vehicle } from './Vehicle';

export class User {
    constructor(
      public id: string,
      public userName: string,
      public fullName: string,
      public email: string,
      public vehicles: Vehicle[],
    ) {}
  }
