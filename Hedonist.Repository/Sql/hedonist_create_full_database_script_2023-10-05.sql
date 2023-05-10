--
-- PostgreSQL database dump
--

-- Dumped from database version 11.19 (Debian 11.19-0+deb10u1)
-- Dumped by pg_dump version 15.2

-- Started on 2023-05-10 22:43:20

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 7 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: postgres
--

-- *not* creating schema, since initdb creates it


ALTER SCHEMA public OWNER TO postgres;

--
-- TOC entry 277 (class 1255 OID 18281)
-- Name: get_gift_group_stores(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.get_gift_group_stores(gift_type_id integer, store_id integer) RETURNS TABLE("GroupTypeId" integer, "GiftsCount" integer, "Comment" character varying, "Id" integer, "Name" character varying)
    LANGUAGE plpgsql
    AS $$
begin

drop table if exists groups_temp;
create temp table groups_temp
as 
SELECT gg."Id", gg."GiftsCount", gg."Comment"
	FROM public."GiftGroup" gg 
	inner join public."GiftTypeStore" gts on gts."GiftGroupId" = gg."Id"
	where gts."GiftTypesId" = gift_type_id and  gts."StoresId" = store_id
;

drop table if exists stores_temp;
create temp table stores_temp
as
select grpt."Id" as "GroupTypeId", grpt."GiftsCount", grpt."Comment", s."Id", s."Name" 
	from public."Store" s
	inner join public."GiftTypeStore" gts on s."Id"  = gts."StoresId" 
	inner join groups_temp grpt on gts."GiftGroupId" = grpt."Id"
	--where gts."GiftGroupId" = grpt."Id"
;
return query
select * from stores_temp;

end;
$$;


ALTER FUNCTION public.get_gift_group_stores(gift_type_id integer, store_id integer) OWNER TO postgres;

--
-- TOC entry 278 (class 1255 OID 21657)
-- Name: get_gifts_reports(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.get_gifts_reports() RETURNS TABLE("GroupTypeId" integer, "GiftsCount" integer, "Comment" character varying, "Id" integer, "Name" character varying)
    LANGUAGE plpgsql
    AS $$
begin

drop table if exists groups_temp;
create temp table groups_temp
as 
SELECT gg."Id", gg."GiftsCount", gg."Comment"
	FROM public."GiftGroup" gg 
	inner join public."GiftTypeStore" gts on gts."GiftGroupId" = gg."Id"
;

drop table if exists stores_temp;
create temp table stores_temp
as
select grpt."Id" as "GroupTypeId", grpt."GiftsCount", grpt."Comment", s."Id", s."Name" 
	from public."Store" s
	inner join public."GiftTypeStore" gts on s."Id"  = gts."StoresId" 
	inner join groups_temp grpt on gts."GiftGroupId" = grpt."Id"
;
return query
select * from stores_temp;

end;
$$;


ALTER FUNCTION public.get_gifts_reports() OWNER TO postgres;

--
-- TOC entry 279 (class 1255 OID 25667)
-- Name: get_left_gifts_report(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.get_left_gifts_report() RETURNS TABLE("GiftGroupId" integer, "Comment" character varying, "GiftsLimit" integer, "LeftGifts" integer)
    LANGUAGE plpgsql
    AS $$
BEGIN

RETURN query

SELECT DISTINCT
gg."Id", gg."Comment", gg."GiftsCount", count(gp."Id")::int4
FROM "GiftGroup" gg
INNER JOIN "GiftTypeStore" gts ON gts."GiftGroupId"  = gg."Id" 
INNER JOIN "Terminal" t ON t."StoreId" = gts."StoresId" 
INNER JOIN "LoginAttempt" la ON la."SentDeviceIdentifier" = t."DeviceIdentifier" 
INNER JOIN "GiftPurchase" gp ON gp."LoginAttemptId" = la."Id"
INNER JOIN "Gift" g ON g."Id" = gp."GiftId" 
WHERE gts."GiftTypesId" = g."GiftTypeId"
GROUP BY gg."Id", gg."Comment", gg."GiftsCount"
;
END;
$$;


ALTER FUNCTION public.get_left_gifts_report() OWNER TO postgres;

--
-- TOC entry 262 (class 1255 OID 17032)
-- Name: gift_delete(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.gift_delete() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
  BEGIN
     INSERT INTO "GiftHistory"
        ("GiftId", "CertificateCode", "GiftTypeId", "IsSold", "OperationType", "ChangedDate")
      VALUES
        (OLD."Id", OLD."CertificateCode", OLD."GiftTypeId", OLD."IsSold", 'delete', now());
    RETURN NEW;

  END;
$$;


ALTER FUNCTION public.gift_delete() OWNER TO postgres;

--
-- TOC entry 263 (class 1255 OID 17033)
-- Name: gift_insert(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.gift_insert() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
    BEGIN
      INSERT INTO "GiftHistory"
        ("GiftId", "CertificateCode", "GiftTypeId", "IsSold", "OperationType", "ChangedDate")
      VALUES
        (NEW."Id", NEW."CertificateCode", NEW."GiftTypeId", new."IsSold", 'insert', now());
      RETURN NEW;
    END;
  $$;


ALTER FUNCTION public.gift_insert() OWNER TO postgres;

--
-- TOC entry 276 (class 1255 OID 17034)
-- Name: gift_update(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.gift_update() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
  BEGIN
     INSERT INTO "GiftHistory"
        ("GiftId", "CertificateCode", "GiftTypeId", "IsSold", "OperationType", "ChangedDate")
      VALUES
        (NEW."Id", NEW."CertificateCode", NEW."GiftTypeId", new."IsSold",'update', now());
    RETURN NEW;

  END;
$$;


ALTER FUNCTION public.gift_update() OWNER TO postgres;

--
-- TOC entry 259 (class 1255 OID 17035)
-- Name: password_delete(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.password_delete() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
    BEGIN
       INSERT INTO password_history
          (id, password_hash, is_used, terminal_name, changed_date, operation)
        VALUES
          (OLD.id, OLD.password_hash, OLD.is_used, OLD.terminal_name, now(), 'delete');
      RETURN NEW;
  
    END;
  $$;


ALTER FUNCTION public.password_delete() OWNER TO postgres;

--
-- TOC entry 260 (class 1255 OID 17036)
-- Name: password_insert(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.password_insert() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
    BEGIN
       INSERT INTO password_history
          (id, password_hash, is_used, terminal_name, changed_date, operation)
        VALUES
          (NEW.id, NEW.password_hash, NEW.is_used, NEW.terminal_name, now(), 'insert');
      RETURN NEW;
  
    END;
  $$;


ALTER FUNCTION public.password_insert() OWNER TO postgres;

--
-- TOC entry 261 (class 1255 OID 17037)
-- Name: password_update(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.password_update() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
    BEGIN
       INSERT INTO password_history
          (id, password_hash, is_used, terminal_name, changed_date, operation)
        VALUES
          (NEW.id, NEW.password_hash, NEW.is_used, NEW.terminal_name, now(), 'update');
      RETURN NEW;
  
    END;
  $$;


ALTER FUNCTION public.password_update() OWNER TO postgres;

SET default_tablespace = '';

--
-- TOC entry 230 (class 1259 OID 17038)
-- Name: Answer; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Answer" (
    "Id" integer NOT NULL,
    "ParentAnswerId" integer,
    "Group" integer NOT NULL,
    "Order" integer NOT NULL,
    "Text" character varying NOT NULL,
    "QuestionId" integer NOT NULL
);


ALTER TABLE public."Answer" OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 17044)
-- Name: AnswerGiftType; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AnswerGiftType" (
    "AnswersId" integer NOT NULL,
    "GiftTypesId" integer NOT NULL
);


ALTER TABLE public."AnswerGiftType" OWNER TO postgres;

--
-- TOC entry 258 (class 1259 OID 26569)
-- Name: AnswersGroup; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AnswersGroup" (
    "Id" integer NOT NULL,
    "Comment" character varying
);


ALTER TABLE public."AnswersGroup" OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 17047)
-- Name: gift_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.gift_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public.gift_id_seq OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 17049)
-- Name: Gift; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Gift" (
    "Id" integer DEFAULT nextval('public.gift_id_seq'::regclass) NOT NULL,
    "CertificateCode" character varying NOT NULL,
    "GiftTypeId" integer NOT NULL,
    "IsSold" boolean DEFAULT false NOT NULL,
    "CreatedDate" timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."Gift" OWNER TO postgres;

--
-- TOC entry 256 (class 1259 OID 18245)
-- Name: GiftGroup; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."GiftGroup" (
    "Id" integer NOT NULL,
    "GiftsCount" integer NOT NULL,
    "Comment" character varying
);


ALTER TABLE public."GiftGroup" OWNER TO postgres;

--
-- TOC entry 252 (class 1259 OID 17268)
-- Name: question_history_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.question_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.question_history_id_seq OWNER TO postgres;

--
-- TOC entry 253 (class 1259 OID 17305)
-- Name: GiftHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."GiftHistory" (
    "Id" integer DEFAULT nextval('public.question_history_id_seq'::regclass) NOT NULL,
    "GiftId" integer NOT NULL,
    "CertificateCode" character varying NOT NULL,
    "GiftTypeId" integer NOT NULL,
    "IsSold" boolean NOT NULL,
    "ChangedDate" timestamp with time zone DEFAULT now() NOT NULL,
    "OperationType" character varying NOT NULL
);


ALTER TABLE public."GiftHistory" OWNER TO postgres;

--
-- TOC entry 254 (class 1259 OID 17379)
-- Name: gift_purchase_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.gift_purchase_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public.gift_purchase_seq OWNER TO postgres;

--
-- TOC entry 255 (class 1259 OID 17381)
-- Name: GiftPurchase; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."GiftPurchase" (
    "Id" integer DEFAULT nextval('public.gift_purchase_seq'::regclass) NOT NULL,
    "GiftId" integer NOT NULL,
    "LoginAttemptId" integer NOT NULL,
    "CertificateCode" character varying NOT NULL,
    "CreatedDate" timestamp with time zone NOT NULL
);


ALTER TABLE public."GiftPurchase" OWNER TO postgres;

--
-- TOC entry 234 (class 1259 OID 17058)
-- Name: GiftType; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."GiftType" (
    "Id" integer NOT NULL,
    "Name" character varying NOT NULL,
    "DescriptionPattern" character varying,
    "Key" character varying,
    "HasQrCode" boolean NOT NULL,
    "ExtendedData" jsonb NOT NULL
);


ALTER TABLE public."GiftType" OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 17064)
-- Name: GiftTypeStore; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."GiftTypeStore" (
    "StoresId" integer NOT NULL,
    "GiftTypesId" integer NOT NULL,
    "GiftGroupId" integer
);


ALTER TABLE public."GiftTypeStore" OWNER TO postgres;

--
-- TOC entry 236 (class 1259 OID 17067)
-- Name: login_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.login_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public.login_id_seq OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 17069)
-- Name: LoginAttempt; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."LoginAttempt" (
    "Id" integer DEFAULT nextval('public.login_id_seq'::regclass) NOT NULL,
    "Psw" character varying NOT NULL,
    "Ticket" character varying NOT NULL,
    "CreatedDate" timestamp with time zone NOT NULL,
    "SentTerminalName" character varying NOT NULL,
    "IsSuccess" boolean NOT NULL,
    "SentDeviceIdentifier" character varying NOT NULL,
    "IsExpired" boolean NOT NULL
);


ALTER TABLE public."LoginAttempt" OWNER TO postgres;

--
-- TOC entry 257 (class 1259 OID 25692)
-- Name: QuestionsGroup; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."QuestionsGroup" (
    "Id" integer NOT NULL,
    "Comment" character varying
);


ALTER TABLE public."QuestionsGroup" OWNER TO postgres;

--
-- TOC entry 238 (class 1259 OID 17076)
-- Name: Store; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Store" (
    "Id" integer NOT NULL,
    "Name" character varying NOT NULL
);


ALTER TABLE public."Store" OWNER TO postgres;

--
-- TOC entry 239 (class 1259 OID 17082)
-- Name: Terminal; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Terminal" (
    "Id" integer NOT NULL,
    "Name" character varying NOT NULL,
    "DeviceIdentifier" character varying NOT NULL,
    "StoreId" integer,
    "QuestionsGroupId" integer,
    "AnswersGroupId" integer
);


ALTER TABLE public."Terminal" OWNER TO postgres;

--
-- TOC entry 240 (class 1259 OID 17088)
-- Name: answer_gift_type_relations_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.answer_gift_type_relations_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.answer_gift_type_relations_seq OWNER TO postgres;

--
-- TOC entry 241 (class 1259 OID 17090)
-- Name: answer_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.answer_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.answer_id_seq OWNER TO postgres;

--
-- TOC entry 3120 (class 0 OID 0)
-- Dependencies: 241
-- Name: answer_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.answer_id_seq OWNED BY public."Answer"."Id";


--
-- TOC entry 242 (class 1259 OID 17092)
-- Name: gift_relations_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.gift_relations_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    MAXVALUE 2147483647
    CACHE 1;


ALTER TABLE public.gift_relations_seq OWNER TO postgres;

--
-- TOC entry 243 (class 1259 OID 17094)
-- Name: logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.logs (
    id integer NOT NULL,
    application character varying(100),
    logged text,
    level character varying(100),
    message character varying(8000),
    logger character varying(8000),
    callsite character varying(8000),
    exception character varying(8000)
);


ALTER TABLE public.logs OWNER TO postgres;

--
-- TOC entry 244 (class 1259 OID 17100)
-- Name: logs_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.logs_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.logs_id_seq OWNER TO postgres;

--
-- TOC entry 3121 (class 0 OID 0)
-- Dependencies: 244
-- Name: logs_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.logs_id_seq OWNED BY public.logs.id;


--
-- TOC entry 245 (class 1259 OID 17102)
-- Name: password_history; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.password_history (
    id integer NOT NULL,
    password_hash character varying NOT NULL,
    is_used boolean NOT NULL,
    changed_date timestamp without time zone NOT NULL,
    operation character varying NOT NULL,
    terminal_name character varying
);


ALTER TABLE public.password_history OWNER TO postgres;

--
-- TOC entry 246 (class 1259 OID 17108)
-- Name: password_info; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.password_info (
    id integer NOT NULL,
    password_hash character varying NOT NULL,
    is_used boolean NOT NULL,
    ticket character varying,
    terminal_name character varying NOT NULL
);


ALTER TABLE public.password_info OWNER TO postgres;

--
-- TOC entry 247 (class 1259 OID 17114)
-- Name: password_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.password_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.password_id_seq OWNER TO postgres;

--
-- TOC entry 3122 (class 0 OID 0)
-- Dependencies: 247
-- Name: password_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.password_id_seq OWNED BY public.password_info.id;


--
-- TOC entry 248 (class 1259 OID 17116)
-- Name: question; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.question (
    id integer NOT NULL,
    "group" integer NOT NULL,
    "order" integer NOT NULL,
    text character varying NOT NULL
);


ALTER TABLE public.question OWNER TO postgres;

--
-- TOC entry 249 (class 1259 OID 17122)
-- Name: question_history; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.question_history (
    id integer NOT NULL,
    "group" integer NOT NULL,
    "order" integer NOT NULL,
    text character varying NOT NULL,
    created_date timestamp without time zone NOT NULL,
    operation character varying NOT NULL
);


ALTER TABLE public.question_history OWNER TO postgres;

--
-- TOC entry 250 (class 1259 OID 17128)
-- Name: question_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.question_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.question_id_seq OWNER TO postgres;

--
-- TOC entry 3123 (class 0 OID 0)
-- Dependencies: 250
-- Name: question_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.question_id_seq OWNED BY public.question.id;


--
-- TOC entry 251 (class 1259 OID 17130)
-- Name: store_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.store_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.store_id_seq OWNER TO postgres;

--
-- TOC entry 2926 (class 2604 OID 17132)
-- Name: logs id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.logs ALTER COLUMN id SET DEFAULT nextval('public.logs_id_seq'::regclass);


--
-- TOC entry 2927 (class 2604 OID 17133)
-- Name: password_info id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.password_info ALTER COLUMN id SET DEFAULT nextval('public.password_id_seq'::regclass);


--
-- TOC entry 2928 (class 2604 OID 17134)
-- Name: question id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.question ALTER COLUMN id SET DEFAULT nextval('public.question_id_seq'::regclass);


--
-- TOC entry 2935 (class 2606 OID 17136)
-- Name: AnswerGiftType answer_gift_type_relations_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AnswerGiftType"
    ADD CONSTRAINT answer_gift_type_relations_pk PRIMARY KEY ("AnswersId", "GiftTypesId");


--
-- TOC entry 2971 (class 2606 OID 26576)
-- Name: AnswersGroup answer_group_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AnswersGroup"
    ADD CONSTRAINT answer_group_pk PRIMARY KEY ("Id");


--
-- TOC entry 2933 (class 2606 OID 17138)
-- Name: Answer answer_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Answer"
    ADD CONSTRAINT answer_pk PRIMARY KEY ("Id");


--
-- TOC entry 2967 (class 2606 OID 18252)
-- Name: GiftGroup gift_group_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GiftGroup"
    ADD CONSTRAINT gift_group_pk PRIMARY KEY ("Id");


--
-- TOC entry 2937 (class 2606 OID 17140)
-- Name: Gift gift_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Gift"
    ADD CONSTRAINT gift_pk PRIMARY KEY ("Id");


--
-- TOC entry 2965 (class 2606 OID 17389)
-- Name: GiftPurchase gift_purchase_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GiftPurchase"
    ADD CONSTRAINT gift_purchase_pk PRIMARY KEY ("Id");


--
-- TOC entry 2939 (class 2606 OID 17142)
-- Name: GiftType gift_type_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GiftType"
    ADD CONSTRAINT gift_type_pk PRIMARY KEY ("Id");


--
-- TOC entry 2941 (class 2606 OID 17144)
-- Name: GiftType gifttype_key_un; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GiftType"
    ADD CONSTRAINT gifttype_key_un UNIQUE ("Key");


--
-- TOC entry 2943 (class 2606 OID 17146)
-- Name: GiftType gifttype_name_un; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GiftType"
    ADD CONSTRAINT gifttype_name_un UNIQUE ("Name");


--
-- TOC entry 2959 (class 2606 OID 17148)
-- Name: password_info hash_unique; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.password_info
    ADD CONSTRAINT hash_unique UNIQUE (password_hash);


--
-- TOC entry 2947 (class 2606 OID 17150)
-- Name: LoginAttempt login_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."LoginAttempt"
    ADD CONSTRAINT login_pk PRIMARY KEY ("Id");


--
-- TOC entry 2957 (class 2606 OID 17152)
-- Name: logs logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.logs
    ADD CONSTRAINT logs_pkey PRIMARY KEY (id);


--
-- TOC entry 2961 (class 2606 OID 17154)
-- Name: password_info password_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.password_info
    ADD CONSTRAINT password_pk PRIMARY KEY (id);


--
-- TOC entry 2969 (class 2606 OID 25699)
-- Name: QuestionsGroup question_group_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."QuestionsGroup"
    ADD CONSTRAINT question_group_pk PRIMARY KEY ("Id");


--
-- TOC entry 2963 (class 2606 OID 17156)
-- Name: question question_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.question
    ADD CONSTRAINT question_pk PRIMARY KEY (id);


--
-- TOC entry 2949 (class 2606 OID 17158)
-- Name: Store store_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Store"
    ADD CONSTRAINT store_pk PRIMARY KEY ("Id");


--
-- TOC entry 2945 (class 2606 OID 17160)
-- Name: GiftTypeStore storegifttype_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GiftTypeStore"
    ADD CONSTRAINT storegifttype_pk PRIMARY KEY ("StoresId", "GiftTypesId");


--
-- TOC entry 2951 (class 2606 OID 17162)
-- Name: Terminal terminal_identifier_un; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Terminal"
    ADD CONSTRAINT terminal_identifier_un UNIQUE ("DeviceIdentifier");


--
-- TOC entry 2953 (class 2606 OID 17164)
-- Name: Terminal terminal_name_un; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Terminal"
    ADD CONSTRAINT terminal_name_un UNIQUE ("Name");


--
-- TOC entry 2955 (class 2606 OID 17166)
-- Name: Terminal terminal_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Terminal"
    ADD CONSTRAINT terminal_pk PRIMARY KEY ("Id");


--
-- TOC entry 2987 (class 2620 OID 17263)
-- Name: Gift gift_delete_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER gift_delete_trigger AFTER DELETE ON public."Gift" FOR EACH ROW EXECUTE PROCEDURE public.gift_delete();


--
-- TOC entry 2988 (class 2620 OID 17261)
-- Name: Gift gift_insert_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER gift_insert_trigger AFTER INSERT ON public."Gift" FOR EACH ROW EXECUTE PROCEDURE public.gift_insert();


--
-- TOC entry 2989 (class 2620 OID 17262)
-- Name: Gift gift_update_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER gift_update_trigger AFTER UPDATE ON public."Gift" FOR EACH ROW EXECUTE PROCEDURE public.gift_update();


--
-- TOC entry 2990 (class 2620 OID 17167)
-- Name: password_info password_delete_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER password_delete_trigger AFTER DELETE ON public.password_info FOR EACH ROW EXECUTE PROCEDURE public.password_delete();


--
-- TOC entry 2991 (class 2620 OID 17168)
-- Name: password_info password_insert_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER password_insert_trigger AFTER INSERT ON public.password_info FOR EACH ROW EXECUTE PROCEDURE public.password_insert();


--
-- TOC entry 2992 (class 2620 OID 17169)
-- Name: password_info password_update_trigger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER password_update_trigger AFTER UPDATE ON public.password_info FOR EACH ROW EXECUTE PROCEDURE public.password_update();


--
-- TOC entry 2972 (class 2606 OID 17170)
-- Name: Answer answer_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Answer"
    ADD CONSTRAINT answer_fk FOREIGN KEY ("ParentAnswerId") REFERENCES public."Answer"("Id");


--
-- TOC entry 2975 (class 2606 OID 17175)
-- Name: AnswerGiftType answer_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AnswerGiftType"
    ADD CONSTRAINT answer_fk FOREIGN KEY ("AnswersId") REFERENCES public."Answer"("Id");


--
-- TOC entry 2973 (class 2606 OID 17180)
-- Name: Answer answer_question_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Answer"
    ADD CONSTRAINT answer_question_fk FOREIGN KEY ("QuestionId") REFERENCES public.question(id);


--
-- TOC entry 2974 (class 2606 OID 26577)
-- Name: Answer answers_group_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Answer"
    ADD CONSTRAINT answers_group_fk FOREIGN KEY ("Group") REFERENCES public."AnswersGroup"("Id");


--
-- TOC entry 2977 (class 2606 OID 17185)
-- Name: Gift gift_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Gift"
    ADD CONSTRAINT gift_fk FOREIGN KEY ("GiftTypeId") REFERENCES public."GiftType"("Id");


--
-- TOC entry 2985 (class 2606 OID 17390)
-- Name: GiftPurchase gift_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GiftPurchase"
    ADD CONSTRAINT gift_id_fk FOREIGN KEY ("GiftId") REFERENCES public."Gift"("Id");


--
-- TOC entry 2978 (class 2606 OID 17190)
-- Name: GiftTypeStore gift_relations_gift_type_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GiftTypeStore"
    ADD CONSTRAINT gift_relations_gift_type_fk FOREIGN KEY ("GiftTypesId") REFERENCES public."GiftType"("Id");


--
-- TOC entry 2979 (class 2606 OID 17195)
-- Name: GiftTypeStore gift_relations_store_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GiftTypeStore"
    ADD CONSTRAINT gift_relations_store_fk FOREIGN KEY ("StoresId") REFERENCES public."Store"("Id");


--
-- TOC entry 2976 (class 2606 OID 17200)
-- Name: AnswerGiftType gift_type_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AnswerGiftType"
    ADD CONSTRAINT gift_type_fk FOREIGN KEY ("GiftTypesId") REFERENCES public."GiftType"("Id");


--
-- TOC entry 2980 (class 2606 OID 18274)
-- Name: GiftTypeStore gifttypestore_group_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GiftTypeStore"
    ADD CONSTRAINT gifttypestore_group_fk FOREIGN KEY ("GiftGroupId") REFERENCES public."GiftGroup"("Id");


--
-- TOC entry 2986 (class 2606 OID 17395)
-- Name: GiftPurchase login_attempt_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GiftPurchase"
    ADD CONSTRAINT login_attempt_fk FOREIGN KEY ("LoginAttemptId") REFERENCES public."LoginAttempt"("Id");


--
-- TOC entry 2984 (class 2606 OID 26564)
-- Name: question questions_group_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.question
    ADD CONSTRAINT questions_group_fk FOREIGN KEY ("group") REFERENCES public."QuestionsGroup"("Id");


--
-- TOC entry 2981 (class 2606 OID 26793)
-- Name: Terminal terminal_answers_group_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Terminal"
    ADD CONSTRAINT terminal_answers_group_fk FOREIGN KEY ("AnswersGroupId") REFERENCES public."AnswersGroup"("Id");


--
-- TOC entry 2982 (class 2606 OID 17205)
-- Name: Terminal terminal_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Terminal"
    ADD CONSTRAINT terminal_fk FOREIGN KEY ("StoreId") REFERENCES public."Store"("Id");


--
-- TOC entry 2983 (class 2606 OID 26798)
-- Name: Terminal terminal_questions_group_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Terminal"
    ADD CONSTRAINT terminal_questions_group_fk FOREIGN KEY ("QuestionsGroupId") REFERENCES public."QuestionsGroup"("Id");


--
-- TOC entry 3119 (class 0 OID 0)
-- Dependencies: 7
-- Name: SCHEMA public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE USAGE ON SCHEMA public FROM PUBLIC;
GRANT ALL ON SCHEMA public TO PUBLIC;


-- Completed on 2023-05-10 22:43:27

--
-- PostgreSQL database dump complete
--

