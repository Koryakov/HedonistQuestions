-- DROP SEQUENCE public.gift_id_seq;
CREATE SEQUENCE public.gift_id_seq
	INCREMENT BY 1
	MINVALUE 1
	MAXVALUE 2147483647
	START 1
	CACHE 1
	NO CYCLE;

--DROP TABLE public.gift;
CREATE TABLE public.gift (
	id int4 NOT NULL DEFAULT nextval('gift_id_seq'::regclass),
	sertificate varchar NOT NULL,
	CONSTRAINT gift_pk PRIMARY KEY (id)
);


-- DROP SEQUENCE public.gift_history_gift_id_seq;
CREATE SEQUENCE public.gift_history_gift_id_seq
	INCREMENT BY 1
	MINVALUE 1
	MAXVALUE 2147483647
	START 1
	CACHE 1
	NO CYCLE;

--DROP TABLE public.gift_history;
CREATE TABLE public.gift_history (
	id int4 NOT NULL DEFAULT nextval('gift_history_gift_id_seq'::regclass),
	gift_id int4 NOT NULL,
	sertificate varchar NOT NULL,
	changed_date timestamp NOT NULL,
	operation_type varchar NOT NULL
);


-- DROP SEQUENCE public.gift_relations_seq;
CREATE SEQUENCE public.gift_relations_seq
	INCREMENT BY 1
	MINVALUE 1
	MAXVALUE 2147483647
	START 1
	CACHE 1
	NO CYCLE;

--DROP TABLE public.gift_relations;
CREATE TABLE public.gift_relations (
	id int4 NOT NULL DEFAULT nextval('gift_relations_seq'::regclass),
	store_id int4 NOT NULL,
	gift_type_id int4 NOT null,
	CONSTRAINT gift_relations_pk PRIMARY KEY (id)
);

ALTER TABLE public.gift_relations ADD CONSTRAINT gift_relations_store_fk FOREIGN KEY (store_id) REFERENCES public.store(id);
ALTER TABLE public.gift_relations ADD CONSTRAINT gift_relations_gift_type_fk FOREIGN KEY (gift_type_id) REFERENCES public.gift_type(id);

--FUNCTIONS
/*
CREATE OR REPLACE FUNCTION public.gift_delete()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
  BEGIN
     INSERT INTO gift_history
        (id, remaining_count, sertificate_code, "text", operation_type, changed_date)
      VALUES
        (OLD.id, OLD.remaining_count, OLD.sertificate_code, OLD."text", 'delete', now());
    RETURN NEW;

  END;
$function$
;

CREATE OR REPLACE FUNCTION public.gift_insert()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
    BEGIN
      INSERT INTO gift_history
        (id, remaining_count, sertificate_code, "text", operation_type, changed_date)
      VALUES
        (NEW.id, NEW.remaining_count, NEW.sertificate_code, NEW."text", 'insert', now());
      RETURN NEW;
    END;
  $function$
;

CREATE OR REPLACE FUNCTION public.gift_update()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
  BEGIN
     INSERT INTO gift_history
        (id, remaining_count, sertificate_code, "text", operation_type, changed_date)
      VALUES
        (NEW.id, NEW.remaining_count, NEW.sertificate_code, NEW."text", 'update', now());
    RETURN NEW;

  END;
$function$
;
*/



--TRIGGERS
/*
create trigger gift_insert_trigger after
insert
    on
    public.gift for each row execute procedure gift_insert();
create trigger gift_update_trigger after
update
    on
    public.gift for each row execute procedure gift_update();
create trigger gift_delete_trigger after
delete
    on
    public.gift for each row execute procedure gift_delete();
*/