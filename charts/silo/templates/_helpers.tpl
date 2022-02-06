{{/*
Expand the name of the chart.
*/}}
{{- define "silo.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "silo.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "silo.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "silo.labels" -}}
helm.sh/chart: {{ include "silo.chart" . }}
{{ include "silo.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
orleans/serviceId: orleans-silo
orleans/clusterId: silo-orleans-cluster
{{- end }}

{{/*
Selector labels
*/}}
{{- define "silo.selectorLabels" -}}
app.kubernetes.io/name: {{ include "silo.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
orleans/serviceId: orleans-silo
orleans/clusterId: silo-orleans-cluster
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "silo.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "silo.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}
