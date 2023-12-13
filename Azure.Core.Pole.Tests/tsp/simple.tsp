import "@typespec/json-schema";

using TypeSpec.JsonSchema;

@jsonSchema
namespace Schemas;

model Person {
  firstName: string;
  lastName: string;
  age: int32;
  address: Address;
  nickNames?: string[];
  cars?: Car[];
}

model Address {
  street: string;
  city: string;
  country: string;
}
model Car {
  kind: "ev" | "ice";
  brand: string;
  model: string;
}