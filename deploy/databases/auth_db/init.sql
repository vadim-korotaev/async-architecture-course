create table user_roles (
    id serial PRIMARY KEY,
    role_name varchar
);

INSERT INTO user_roles (id, role_name) values
    (1, 'Worker'),
    (2, 'Admin'),
    (3, 'Manager');

create table users (
	id serial PRIMARY KEY,
	username varchar,
    password varchar,
    role_id int not null,
	is_deleted boolean default false,
    public_id uuid not null,

	CONSTRAINT user_role_id_fkey FOREIGN KEY (role_id) REFERENCES user_roles(id)
);
