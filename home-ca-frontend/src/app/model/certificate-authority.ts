export class CertificateAuthority {
    public name : string | undefined;
    public description : string | undefined;
    public hasChildren : boolean = false;
    public links: { [name: string]: string; } | undefined;
}
